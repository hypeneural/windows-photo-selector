using System.Diagnostics;

namespace Evydencia.PhotoSelector.Launcher;

public static class PhotoSelectorLaunchCommand
{
    public static ProcessStartInfo CreateStartInfo(
        string appPath,
        string folderPath,
        string source)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = appPath,
            UseShellExecute = false
        };

        startInfo.ArgumentList.Add("--folder");
        startInfo.ArgumentList.Add(folderPath);
        startInfo.ArgumentList.Add("--source");
        startInfo.ArgumentList.Add(source);

        return startInfo;
    }
}
