namespace Evydencia.PhotoSelector.Core.Photos;

public enum PhotoStatus
{
    Active,
    PendingDelete,
    Deleted,
    PendingRestore,
    Restored,
    Missing,
    DeleteFailed
}
