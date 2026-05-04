using System.Diagnostics;

namespace Evydencia.PhotoSelector.Launcher;

public sealed class PhotoSelectorLauncher
{
    private readonly LauncherArgumentsParser _argumentsParser;
    private readonly AppPathResolver _appPathResolver;
    private readonly Action<string> _writeError;

    public PhotoSelectorLauncher(
        LauncherArgumentsParser argumentsParser,
        AppPathResolver appPathResolver,
        Action<string> writeError)
    {
        _argumentsParser = argumentsParser;
        _appPathResolver = appPathResolver;
        _writeError = writeError;
    }

    public static int Run(IReadOnlyList<string> arguments, Action<string> writeError)
    {
        var launcher = new PhotoSelectorLauncher(
            new LauncherArgumentsParser(),
            new AppPathResolver(),
            writeError);

        return (int)launcher.Execute(arguments);
    }

    public LauncherExitCode Execute(IReadOnlyList<string> arguments)
    {
        var parseResult = _argumentsParser.Parse(arguments);
        if (!parseResult.IsSuccess || parseResult.Options is null)
        {
            _writeError(parseResult.ErrorMessage ?? "Argumentos invalidos.");
            return LauncherExitCode.InvalidArguments;
        }

        var options = parseResult.Options;
        if (options.ShowHelp)
        {
            _writeError("Uso: Evydencia.PhotoSelector.Launcher --folder <pasta> --app <app.exe>");
            return LauncherExitCode.Success;
        }

        if (!options.HasFolder)
        {
            _writeError("Informe uma pasta com --folder ou como primeiro argumento.");
            return LauncherExitCode.InvalidArguments;
        }

        var normalizedFolderPath = NormalizeFolderPath(options.FolderPath!);
        if (normalizedFolderPath is null)
        {
            _writeError($"Pasta invalida ou inexistente: {options.FolderPath}");
            return LauncherExitCode.FolderNotFound;
        }

        var appPath = _appPathResolver.Resolve(options.AppPath);
        if (appPath is null)
        {
            _writeError(
                $"Nao foi possivel localizar Evydencia.PhotoSelector.App.exe. Informe --app ou {AppPathResolver.AppPathEnvironmentVariable}.");
            return LauncherExitCode.AppNotFound;
        }

        try
        {
            using var process = Process.Start(PhotoSelectorLaunchCommand.CreateStartInfo(
                appPath,
                normalizedFolderPath,
                options.Source));

            return process is null
                ? LauncherExitCode.LaunchFailed
                : LauncherExitCode.Success;
        }
        catch (Exception exception) when (exception is InvalidOperationException or IOException or System.ComponentModel.Win32Exception)
        {
            _writeError($"Falha ao iniciar o app: {exception.Message}");
            return LauncherExitCode.LaunchFailed;
        }
    }

    private static string? NormalizeFolderPath(string folderPath)
    {
        try
        {
            var fullPath = Path.GetFullPath(folderPath);
            return Directory.Exists(fullPath)
                ? fullPath
                : null;
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return null;
        }
    }
}
