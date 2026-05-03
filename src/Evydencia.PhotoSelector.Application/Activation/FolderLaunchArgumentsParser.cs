using System.Text;

namespace Evydencia.PhotoSelector.Application.Activation;

public sealed class FolderLaunchArgumentsParser
{
    public FolderLaunchArguments Parse(IEnumerable<string>? arguments)
    {
        if (arguments is null)
        {
            return FolderLaunchArguments.Empty;
        }

        string? folderPath = null;
        string? source = null;
        var tokens = arguments
            .Where(argument => !string.IsNullOrWhiteSpace(argument))
            .Select(argument => argument.Trim())
            .ToArray();

        for (var index = 0; index < tokens.Length; index++)
        {
            var token = tokens[index];
            if (TryReadInlineValue(token, "--folder=", out var inlineFolder)
                || TryReadInlineValue(token, "/folder=", out inlineFolder)
                || TryReadInlineValue(token, "/folder:", out inlineFolder))
            {
                folderPath = inlineFolder;
                continue;
            }

            if (IsFolderSwitch(token) && TryReadNextValue(tokens, index, out var nextFolder))
            {
                folderPath = nextFolder;
                index++;
                continue;
            }

            if (TryReadInlineValue(token, "--source=", out var inlineSource)
                || TryReadInlineValue(token, "/source=", out inlineSource)
                || TryReadInlineValue(token, "/source:", out inlineSource))
            {
                source = inlineSource;
                continue;
            }

            if (IsSourceSwitch(token) && TryReadNextValue(tokens, index, out var nextSource))
            {
                source = nextSource;
                index++;
            }
        }

        return string.IsNullOrWhiteSpace(folderPath) && string.IsNullOrWhiteSpace(source)
            ? FolderLaunchArguments.Empty
            : new FolderLaunchArguments(folderPath, source);
    }

    public FolderLaunchArguments ParseRaw(string? rawArguments)
    {
        return Parse(Tokenize(rawArguments));
    }

    private static List<string> Tokenize(string? rawArguments)
    {
        if (string.IsNullOrWhiteSpace(rawArguments))
        {
            return [];
        }

        var tokens = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        foreach (var character in rawArguments)
        {
            if (character == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (char.IsWhiteSpace(character) && !inQuotes)
            {
                AddToken(tokens, current);
                continue;
            }

            current.Append(character);
        }

        AddToken(tokens, current);
        return tokens;
    }

    private static void AddToken(List<string> tokens, StringBuilder current)
    {
        if (current.Length == 0)
        {
            return;
        }

        tokens.Add(current.ToString());
        current.Clear();
    }

    private static bool IsFolderSwitch(string token)
    {
        return string.Equals(token, "--folder", StringComparison.OrdinalIgnoreCase)
            || string.Equals(token, "/folder", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSourceSwitch(string token)
    {
        return string.Equals(token, "--source", StringComparison.OrdinalIgnoreCase)
            || string.Equals(token, "/source", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryReadInlineValue(string token, string prefix, out string? value)
    {
        if (token.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            value = NormalizeValue(token[prefix.Length..]);
            return !string.IsNullOrWhiteSpace(value);
        }

        value = null;
        return false;
    }

    private static bool TryReadNextValue(string[] tokens, int currentIndex, out string? value)
    {
        var nextIndex = currentIndex + 1;
        if (nextIndex >= tokens.Length || IsSwitch(tokens[nextIndex]))
        {
            value = null;
            return false;
        }

        value = NormalizeValue(tokens[nextIndex]);
        return !string.IsNullOrWhiteSpace(value);
    }

    private static bool IsSwitch(string token)
    {
        return token.StartsWith("--", StringComparison.Ordinal)
            || token.StartsWith('/');
    }

    private static string? NormalizeValue(string value)
    {
        var normalized = value.Trim();
        return normalized.Length == 0 ? null : normalized;
    }
}
