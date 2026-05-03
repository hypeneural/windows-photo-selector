using Evydencia.PhotoSelector.Core.Scanning;
using Evydencia.PhotoSelector.Core.Sessions;

namespace Evydencia.PhotoSelector.Application.Tests;

internal static class SessionFactory
{
    public static PhotoSession Create(params string[] fileNames)
    {
        var candidates = fileNames.Select((fileName, index) => new PhotoFileCandidate(
            fileName,
            $"C:\\sessao\\{fileName}",
            "C:\\sessao",
            fileName[fileName.LastIndexOf('.')..],
            1024,
            DateTimeOffset.UtcNow,
            index));

        return new PhotoSessionFactory().Create("C:\\sessao", candidates);
    }
}
