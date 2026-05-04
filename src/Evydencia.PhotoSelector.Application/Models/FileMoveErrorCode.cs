namespace Evydencia.PhotoSelector.Application.Models;

public enum FileMoveErrorCode
{
    None,
    SourceMissing,
    AccessDenied,
    InvalidPath,
    PathTooLong,
    IoFailure,
    Unknown
}
