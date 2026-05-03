using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

var artifactsPath = Path.Combine(FindRepositoryRoot(), "artifacts", "performance");
Directory.CreateDirectory(artifactsPath);

var config = DefaultConfig.Instance.WithArtifactsPath(artifactsPath);
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);

static string FindRepositoryRoot()
{
    var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "Evydencia.PhotoSelector.sln")))
        {
            return directory.FullName;
        }

        directory = directory.Parent;
    }

    return Directory.GetCurrentDirectory();
}
