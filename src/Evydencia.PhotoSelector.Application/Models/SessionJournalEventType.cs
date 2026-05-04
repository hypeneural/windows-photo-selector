namespace Evydencia.PhotoSelector.Application.Models;

public static class SessionJournalEventType
{
    public const string DeleteRequested = nameof(DeleteRequested);
    public const string Deleted = nameof(Deleted);
    public const string DeleteFailed = nameof(DeleteFailed);
    public const string UndoRequested = nameof(UndoRequested);
    public const string Restored = nameof(Restored);
    public const string RestoreFailed = nameof(RestoreFailed);
}
