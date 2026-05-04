namespace Evydencia.PhotoSelector.Application.Models;

public enum FileMoveErrorCode
{
    None,
    SourceMissing,
    FileLocked,
    AccessDenied,
    InvalidPath,
    PathTooLong,
    IoFailure,
    Unknown
}
