using Evydencia.PhotoSelector.Application.Activation;
using Evydencia.PhotoSelector.Application.Models;

namespace Evydencia.PhotoSelector.Application.UseCases;

public sealed class OpenFolderFromArgumentsUseCase
{
    private readonly FolderLaunchArgumentsParser _argumentsParser;
    private readonly OpenSessionUseCase _openSessionUseCase;

    public OpenFolderFromArgumentsUseCase(
        FolderLaunchArgumentsParser argumentsParser,
        OpenSessionUseCase openSessionUseCase)
    {
        _argumentsParser = argumentsParser;
        _openSessionUseCase = openSessionUseCase;
    }

    public async Task<OpenFolderFromArgumentsResult> ExecuteAsync(
        IEnumerable<string>? arguments,
        CancellationToken cancellationToken = default)
    {
        var launchArguments = _argumentsParser.Parse(arguments);
        return await ExecuteAsync(launchArguments, cancellationToken).ConfigureAwait(false);
    }

    public async Task<OpenFolderFromArgumentsResult> ExecuteRawAsync(
        string? rawArguments,
        CancellationToken cancellationToken = default)
    {
        var launchArguments = _argumentsParser.ParseRaw(rawArguments);
        return await ExecuteAsync(launchArguments, cancellationToken).ConfigureAwait(false);
    }

    private async Task<OpenFolderFromArgumentsResult> ExecuteAsync(
        FolderLaunchArguments launchArguments,
        CancellationToken cancellationToken)
    {
        if (!launchArguments.HasFolder)
        {
            return OpenFolderFromArgumentsResult.NoFolderArgument(launchArguments);
        }

        try
        {
            var sessionResult = await _openSessionUseCase.ExecuteAsync(
                new OpenSessionCommand(launchArguments.FolderPath!),
                cancellationToken).ConfigureAwait(false);

            return OpenFolderFromArgumentsResult.Opened(launchArguments, sessionResult);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return OpenFolderFromArgumentsResult.Failed(launchArguments, exception);
        }
    }
}
