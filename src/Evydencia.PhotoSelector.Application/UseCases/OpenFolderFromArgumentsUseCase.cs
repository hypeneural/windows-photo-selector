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
