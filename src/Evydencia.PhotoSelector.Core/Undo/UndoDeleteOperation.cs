namespace Evydencia.PhotoSelector.Core.Undo;

public sealed class UndoDeleteOperation
{
    public UndoDeleteOperation(
        Guid sessionId,
        Guid photoId,
        string originalPath,
        string deletedPath)
    {
        if (sessionId == Guid.Empty)
        {
            throw new ArgumentException("Session id cannot be empty.", nameof(sessionId));
        }

        if (photoId == Guid.Empty)
        {
            throw new ArgumentException("Photo id cannot be empty.", nameof(photoId));
        }

        SessionId = sessionId;
        PhotoId = photoId;
        OriginalPath = Required(originalPath, nameof(originalPath));
        DeletedPath = Required(deletedPath, nameof(deletedPath));
    }

    public Guid SessionId { get; }

    public Guid PhotoId { get; }

    public string OriginalPath { get; }

    public string DeletedPath { get; }

    private static string Required(string value, string parameterName)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value cannot be empty.", parameterName)
            : value;
    }
}
