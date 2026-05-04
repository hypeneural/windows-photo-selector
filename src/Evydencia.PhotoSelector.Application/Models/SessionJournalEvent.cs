using Evydencia.PhotoSelector.Core.Photos;
using Evydencia.PhotoSelector.Core.Sessions;

namespace Evydencia.PhotoSelector.Application.Models;

public sealed class SessionJournalEvent
{
    private SessionJournalEvent(
        Guid eventId,
        Guid sessionId,
        string eventType,
        Guid? photoId,
        string? fileName,
        string? originalPath,
        string? deletedPath,
        string? actualPath,
        string source,
        bool undoable,
        string? errorCode,
        string? errorMessage,
        DateTimeOffset createdAt)
    {
        EventId = eventId;
        SessionId = sessionId;
        EventType = eventType;
        PhotoId = photoId;
        FileName = fileName;
        OriginalPath = originalPath;
        DeletedPath = deletedPath;
        ActualPath = actualPath;
        Source = source;
        Undoable = undoable;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        CreatedAt = createdAt;
    }

    public Guid EventId { get; }

    public Guid SessionId { get; }

    public string EventType { get; }

    public Guid? PhotoId { get; }

    public string? FileName { get; }

    public string? OriginalPath { get; }

    public string? DeletedPath { get; }

    public string? ActualPath { get; }

    public string Source { get; }

    public bool Undoable { get; }

    public string? ErrorCode { get; }

    public string? ErrorMessage { get; }

    public DateTimeOffset CreatedAt { get; }

    public static SessionJournalEvent DeleteRequested(
        PhotoSession session,
        PhotoItem photo,
        string source = "Application")
    {
        return Create(
            session,
            SessionJournalEventType.DeleteRequested,
            photo,
            originalPath: photo.FullPath,
            deletedPath: null,
            actualPath: null,
            source,
            undoable: false,
            errorCode: null,
            errorMessage: null);
    }

    public static SessionJournalEvent Deleted(
        PhotoSession session,
        PhotoItem photo,
        FileMoveResult moveResult,
        string source = "Application")
    {
        return Create(
            session,
            SessionJournalEventType.Deleted,
            photo,
            originalPath: moveResult.SourcePath,
            deletedPath: moveResult.ActualDestinationPath ?? moveResult.RequestedDestinationPath,
            actualPath: moveResult.ActualDestinationPath,
            source,
            undoable: true,
            errorCode: null,
            errorMessage: null);
    }

    public static SessionJournalEvent DeleteFailed(
        PhotoSession session,
        PhotoItem photo,
        FileMoveResult moveResult,
        string source = "Application")
    {
        return Create(
            session,
            SessionJournalEventType.DeleteFailed,
            photo,
            originalPath: moveResult.SourcePath,
            deletedPath: moveResult.RequestedDestinationPath,
            actualPath: moveResult.ActualDestinationPath,
            source,
            undoable: false,
            errorCode: moveResult.ErrorCode.ToString(),
            errorMessage: moveResult.ErrorMessage);
    }

    public static SessionJournalEvent UndoRequested(
        PhotoSession session,
        PhotoItem photo,
        string deletedPath,
        string originalPath,
        string source = "Application")
    {
        return Create(
            session,
            SessionJournalEventType.UndoRequested,
            photo,
            originalPath,
            deletedPath,
            actualPath: null,
            source,
            undoable: false,
            errorCode: null,
            errorMessage: null);
    }

    public static SessionJournalEvent Restored(
        PhotoSession session,
        PhotoItem photo,
        FileMoveResult moveResult,
        string source = "Application")
    {
        return Create(
            session,
            SessionJournalEventType.Restored,
            photo,
            originalPath: moveResult.RequestedDestinationPath,
            deletedPath: moveResult.SourcePath,
            actualPath: moveResult.ActualDestinationPath,
            source,
            undoable: false,
            errorCode: null,
            errorMessage: null);
    }

    public static SessionJournalEvent RestoreFailed(
        PhotoSession session,
        PhotoItem photo,
        FileMoveResult moveResult,
        string source = "Application")
    {
        return Create(
            session,
            SessionJournalEventType.RestoreFailed,
            photo,
            originalPath: moveResult.RequestedDestinationPath,
            deletedPath: moveResult.SourcePath,
            actualPath: moveResult.ActualDestinationPath,
            source,
            undoable: true,
            errorCode: moveResult.ErrorCode.ToString(),
            errorMessage: moveResult.ErrorMessage);
    }

    private static SessionJournalEvent Create(
        PhotoSession session,
        string eventType,
        PhotoItem photo,
        string? originalPath,
        string? deletedPath,
        string? actualPath,
        string source,
        bool undoable,
        string? errorCode,
        string? errorMessage)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(photo);

        return new SessionJournalEvent(
            Guid.NewGuid(),
            session.Id,
            eventType,
            photo.Id,
            photo.FileName,
            originalPath,
            deletedPath,
            actualPath,
            source,
            undoable,
            errorCode,
            errorMessage,
            DateTimeOffset.UtcNow);
    }
}
