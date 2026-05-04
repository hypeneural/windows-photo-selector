namespace Evydencia.PhotoSelector.Application.Abstractions;

public interface IFileExistenceService
{
    bool Exists(string? path);
}
