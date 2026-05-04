using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Sessions;

namespace Evydencia.PhotoSelector.Core.Undo;

public sealed class UndoManager
{
    private readonly object _gate = new();
    private readonly Dictionary<Guid, Stack<UndoDeleteOperation>> _deleteOperationsBySessionId = [];

    public void RegisterDeletedPhoto(
        PhotoSession session,
        PhotoItem deletedPhoto,
        string deletedPath)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(deletedPhoto);

        EnsurePhotoBelongsToSession(session, deletedPhoto);
        if (deletedPhoto.Status != PhotoStatus.Deleted)
        {
            throw new InvalidOperationException("Only deleted photos can be registered for undo.");
        }

        var operation = new UndoDeleteOperation(
            session.Id,
            deletedPhoto.Id,
            deletedPhoto.FullPath,
            deletedPath);

        lock (_gate)
        {
            GetOrCreateStack(session.Id).Push(operation);
        }
    }

    public bool CanUndo(PhotoSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        lock (_gate)
        {
            return _deleteOperationsBySessionId.TryGetValue(session.Id, out var operations)
                && operations.Count > 0;
        }
    }

    public UndoRestoreRequestResult RequestRestoreLast(PhotoSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        var operation = PeekOperation(session.Id);
        if (operation is null)
        {
            return UndoRestoreRequestResult.NoUndoAvailable(session);
        }

        var restoredPhoto = ResolvePhoto(session, operation.PhotoId);
        EnsureDeleted(restoredPhoto);

        var preferredCurrentPhoto = ResolveCurrentPhoto(session);
        restoredPhoto.SetStatus(PhotoStatus.PendingRestore);

        return UndoRestoreRequestResult.PendingRestore(
            session,
            operation,
            restoredPhoto,
            preferredCurrentPhoto);
    }

    public UndoRestoreCompletionResult CompleteRestore(
        PhotoSession session,
        UndoDeleteOperation operation,
        PhotoItem restoredPhoto)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(restoredPhoto);

        EnsureOperationMatchesSession(session, operation);
        EnsurePhotoBelongsToSession(session, restoredPhoto);
        EnsurePendingRestore(restoredPhoto);

        restoredPhoto.SetStatus(PhotoStatus.Restored);
        PopOperation(session.Id, operation);
        SyncCurrentIndex(session, restoredPhoto.Id);

        return new UndoRestoreCompletionResult(
            session,
            UndoRestoreCompletionStatus.Restored,
            operation,
            restoredPhoto,
            ResolveCurrentPhoto(session));
    }

    public UndoRestoreCompletionResult FailRestore(
        PhotoSession session,
        UndoDeleteOperation operation,
        PhotoItem restoredPhoto,
        Guid? preferredCurrentPhotoId)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(restoredPhoto);

        EnsureOperationMatchesSession(session, operation);
        EnsurePhotoBelongsToSession(session, restoredPhoto);
        EnsurePendingRestore(restoredPhoto);

        restoredPhoto.SetStatus(PhotoStatus.Deleted);
        if (preferredCurrentPhotoId.HasValue)
        {
            SyncCurrentIndex(session, preferredCurrentPhotoId.Value);
        }

        return new UndoRestoreCompletionResult(
            session,
            UndoRestoreCompletionStatus.RestoreFailed,
            operation,
            restoredPhoto,
            ResolveCurrentPhoto(session));
    }

    private Stack<UndoDeleteOperation> GetOrCreateStack(Guid sessionId)
    {
        if (!_deleteOperationsBySessionId.TryGetValue(sessionId, out var operations))
        {
            operations = new Stack<UndoDeleteOperation>();
            _deleteOperationsBySessionId.Add(sessionId, operations);
        }

        return operations;
    }

    private UndoDeleteOperation? PeekOperation(Guid sessionId)
    {
        lock (_gate)
        {
            return _deleteOperationsBySessionId.TryGetValue(sessionId, out var operations)
                && operations.Count > 0
                    ? operations.Peek()
                    : null;
        }
    }

    private void PopOperation(Guid sessionId, UndoDeleteOperation operation)
    {
        lock (_gate)
        {
            if (!_deleteOperationsBySessionId.TryGetValue(sessionId, out var operations)
                || operations.Count == 0
                || operations.Peek() != operation)
            {
                throw new InvalidOperationException("Undo operation is not the current operation.");
            }

            operations.Pop();
            if (operations.Count == 0)
            {
                _deleteOperationsBySessionId.Remove(sessionId);
            }
        }
    }

    private static void EnsureOperationMatchesSession(PhotoSession session, UndoDeleteOperation operation)
    {
        if (operation.SessionId != session.Id)
        {
            throw new InvalidOperationException("Undo operation does not belong to the session.");
        }
    }

    private static void EnsurePhotoBelongsToSession(PhotoSession session, PhotoItem photo)
    {
        if (!session.Photos.Any(item => item.Id == photo.Id))
        {
            throw new InvalidOperationException("Photo does not belong to the session.");
        }
    }

    private static void EnsureDeleted(PhotoItem photo)
    {
        if (photo.Status != PhotoStatus.Deleted)
        {
            throw new InvalidOperationException("Photo must be deleted before undo.");
        }
    }

    private static void EnsurePendingRestore(PhotoItem photo)
    {
        if (photo.Status != PhotoStatus.PendingRestore)
        {
            throw new InvalidOperationException("Photo must be pending restore.");
        }
    }

    private static PhotoItem ResolvePhoto(PhotoSession session, Guid photoId)
    {
        return session.Photos.FirstOrDefault(photo => photo.Id == photoId)
            ?? throw new InvalidOperationException("Undo photo does not belong to the session.");
    }

    private static PhotoItem? ResolveCurrentPhoto(PhotoSession session)
    {
        var active = session.ActivePhotos();
        if (active.Count == 0)
        {
            session.CurrentIndex = 0;
            return null;
        }

        if (session.CurrentIndex >= active.Count)
        {
            session.CurrentIndex = active.Count - 1;
        }

        return active[session.CurrentIndex];
    }

    private static void SyncCurrentIndex(PhotoSession session, Guid preferredCurrentPhotoId)
    {
        var index = session.IndexOfActivePhoto(preferredCurrentPhotoId);
        if (index >= 0)
        {
            session.CurrentIndex = index;
        }
    }
}
