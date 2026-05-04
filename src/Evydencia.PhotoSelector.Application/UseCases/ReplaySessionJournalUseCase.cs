using Evydencia.PhotoSelector.Application.Abstractions;
using Evydencia.PhotoSelector.Application.Models;
using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Sessions;

namespace Evydencia.PhotoSelector.Application.UseCases;

public sealed class ReplaySessionJournalUseCase
{
    private readonly IFileExistenceService _fileExistenceService;
    private readonly ISessionJournalStore _journalStore;

    public ReplaySessionJournalUseCase(
        ISessionJournalStore journalStore,
        IFileExistenceService fileExistenceService)
    {
        _journalStore = journalStore;
        _fileExistenceService = fileExistenceService;
    }

    public async Task<SessionJournalReplayResult> ExecuteAsync(
        PhotoSession session,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);
        cancellationToken.ThrowIfCancellationRequested();

        var eventsRead = 0;
        var eventsApplied = 0;
        var recoveredPhotos = 0;
        var missingPhotos = 0;

        await foreach (var journalEvent in _journalStore.ReadEventsAsync(session, cancellationToken)
            .ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();
            eventsRead++;

            if (!TryResolveStatus(journalEvent, out var status))
            {
                continue;
            }

            var photo = ResolvePhoto(session, journalEvent);
            if (photo is null)
            {
                photo = CreateRecoveredPhoto(session, journalEvent, status);
                if (photo is null)
                {
                    continue;
                }

                session.AddRecoveredPhoto(photo);
                recoveredPhotos++;
            }

            ApplyStatusAndLocation(photo, journalEvent, status);
            eventsApplied++;
            if (status == PhotoStatus.Missing)
            {
                missingPhotos++;
            }
        }

        return new SessionJournalReplayResult(
            eventsRead,
            eventsApplied,
            recoveredPhotos,
            missingPhotos);
    }

    private bool TryResolveStatus(SessionJournalEvent journalEvent, out PhotoStatus status)
    {
        var originalExists = _fileExistenceService.Exists(journalEvent.OriginalPath);
        var actualExists = _fileExistenceService.Exists(journalEvent.ActualPath);
        var deletedExists = _fileExistenceService.Exists(journalEvent.DeletedPath);

        status = journalEvent.EventType switch
        {
            SessionJournalEventType.Deleted => ResolveDeletedStatus(originalExists, actualExists || deletedExists),
            SessionJournalEventType.Restored => ResolveRestoredStatus(originalExists || actualExists, deletedExists),
            SessionJournalEventType.DeleteFailed => ResolveDeleteFailedStatus(originalExists || actualExists, deletedExists),
            SessionJournalEventType.RestoreFailed => ResolveRestoredStatus(originalExists || actualExists, deletedExists),
            _ => PhotoStatus.Active
        };

        return journalEvent.EventType is SessionJournalEventType.Deleted
            or SessionJournalEventType.Restored
            or SessionJournalEventType.DeleteFailed
            or SessionJournalEventType.RestoreFailed;
    }

    private static PhotoStatus ResolveDeletedStatus(bool originalExists, bool deletedExists)
    {
        if (originalExists)
        {
            return PhotoStatus.Restored;
        }

        return deletedExists ? PhotoStatus.Deleted : PhotoStatus.Missing;
    }

    private static PhotoStatus ResolveRestoredStatus(bool restoredExists, bool deletedExists)
    {
        if (restoredExists)
        {
            return PhotoStatus.Restored;
        }

        return deletedExists ? PhotoStatus.Deleted : PhotoStatus.Missing;
    }

    private static PhotoStatus ResolveDeleteFailedStatus(bool originalExists, bool deletedExists)
    {
        if (originalExists)
        {
            return PhotoStatus.DeleteFailed;
        }

        return deletedExists ? PhotoStatus.Deleted : PhotoStatus.Missing;
    }

    private static PhotoItem? ResolvePhoto(PhotoSession session, SessionJournalEvent journalEvent)
    {
        var candidatePaths = CandidatePaths(journalEvent).ToArray();
        var byPath = session.Photos.FirstOrDefault(photo =>
            candidatePaths.Any(path => PathsEqual(photo.FullPath, path)));
        if (byPath is not null)
        {
            return byPath;
        }

        if (string.IsNullOrWhiteSpace(journalEvent.FileName))
        {
            return null;
        }

        return session.Photos.FirstOrDefault(photo =>
            string.Equals(photo.FileName, journalEvent.FileName, StringComparison.OrdinalIgnoreCase));
    }

    private static PhotoItem? CreateRecoveredPhoto(
        PhotoSession session,
        SessionJournalEvent journalEvent,
        PhotoStatus status)
    {
        var identityPath = FirstNonEmpty(
            journalEvent.OriginalPath,
            journalEvent.ActualPath,
            journalEvent.DeletedPath);
        var fileName = FirstNonEmpty(journalEvent.FileName, Path.GetFileName(identityPath));
        if (string.IsNullOrWhiteSpace(identityPath) || string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        var originalDirectory = Path.GetDirectoryName(journalEvent.OriginalPath ?? identityPath)
            ?? session.FolderPath;
        var extension = Path.GetExtension(fileName);
        var photoId = ResolveRecoveredPhotoId(session, journalEvent.PhotoId);

        return new PhotoItem(
            photoId,
            fileName,
            identityPath,
            originalDirectory,
            extension,
            sizeBytes: 0,
            journalEvent.CreatedAt,
            sortIndex: session.Photos.Count,
            status);
    }

    private void ApplyStatusAndLocation(
        PhotoItem photo,
        SessionJournalEvent journalEvent,
        PhotoStatus status)
    {
        var restoredPath = FirstExistingPath(journalEvent.ActualPath, journalEvent.OriginalPath);
        if (status is PhotoStatus.Restored or PhotoStatus.DeleteFailed
            && !string.IsNullOrWhiteSpace(restoredPath))
        {
            ApplyLocation(photo, restoredPath);
        }

        photo.SetStatus(status);
    }

    private string? FirstExistingPath(params string?[] paths)
    {
        return paths.FirstOrDefault(_fileExistenceService.Exists);
    }

    private static IEnumerable<string> CandidatePaths(SessionJournalEvent journalEvent)
    {
        foreach (var path in new[] { journalEvent.OriginalPath, journalEvent.ActualPath, journalEvent.DeletedPath })
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                yield return path;
            }
        }
    }

    private static Guid ResolveRecoveredPhotoId(PhotoSession session, Guid? journalPhotoId)
    {
        return journalPhotoId.HasValue
            && journalPhotoId.Value != Guid.Empty
            && session.Photos.All(photo => photo.Id != journalPhotoId.Value)
                ? journalPhotoId.Value
                : Guid.NewGuid();
    }

    private static void ApplyLocation(PhotoItem photo, string path)
    {
        var fileName = Path.GetFileName(path);
        var directory = Path.GetDirectoryName(path);
        var extension = Path.GetExtension(path);
        if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        photo.SetFileLocation(fileName, path, directory, extension);
    }

    private static bool PathsEqual(string left, string right)
    {
        return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }
}
