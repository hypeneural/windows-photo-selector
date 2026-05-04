namespace Evydencia.PhotoSelector.Launcher;

public sealed class LauncherArgumentsParser
{
    public LauncherParseResult Parse(IReadOnlyList<string> arguments)
    {
        string? folderPath = null;
        string? appPath = null;
        var source = LauncherOptions.DefaultSource;
        var showHelp = false;

        for (var index = 0; index < arguments.Count; index++)
        {
            var argument = arguments[index];
            if (IsHelp(argument))
            {
                showHelp = true;
                continue;
            }

            if (IsOption(argument, "--folder"))
            {
                if (!TryReadValue(arguments, ref index, "--folder", out folderPath, out var error))
                {
                    return LauncherParseResult.Failure(error);
                }

                continue;
            }

            if (IsOption(argument, "--app"))
            {
                if (!TryReadValue(arguments, ref index, "--app", out appPath, out var error))
                {
                    return LauncherParseResult.Failure(error);
                }

                continue;
            }

            if (IsOption(argument, "--source"))
            {
                if (!TryReadValue(arguments, ref index, "--source", out var parsedSource, out var error))
                {
                    return LauncherParseResult.Failure(error);
                }

                source = string.IsNullOrWhiteSpace(parsedSource)
                    ? LauncherOptions.DefaultSource
                    : parsedSource;
                continue;
            }

            if (argument.StartsWith("--", StringComparison.Ordinal))
            {
                return LauncherParseResult.Failure($"Argumento desconhecido: {argument}");
            }

            if (folderPath is not null)
            {
                return LauncherParseResult.Failure("Informe apenas uma pasta por ativacao.");
            }

            folderPath = argument;
        }

        return LauncherParseResult.Success(new LauncherOptions(folderPath, appPath, source, showHelp));
    }

    private static bool IsHelp(string argument)
    {
        return string.Equals(argument, "--help", StringComparison.OrdinalIgnoreCase)
            || string.Equals(argument, "-h", StringComparison.OrdinalIgnoreCase)
            || string.Equals(argument, "/?", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsOption(string argument, string option)
    {
        return string.Equals(argument, option, StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryReadValue(
        IReadOnlyList<string> arguments,
        ref int index,
        string option,
        out string? value,
        out string error)
    {
        if (index + 1 >= arguments.Count)
        {
            value = null;
            error = $"O argumento {option} precisa de um valor.";
            return false;
        }

        var nextValue = arguments[index + 1];
        if (nextValue.StartsWith("--", StringComparison.Ordinal))
        {
            value = null;
            error = $"O argumento {option} precisa de um valor.";
            return false;
        }

        index++;
        value = nextValue;
        error = string.Empty;
        return true;
    }
}
