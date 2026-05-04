namespace Evydencia.PhotoSelector.App.Activation;

internal sealed class FolderActivationRequestedEventArgs : EventArgs
{
    public FolderActivationRequestedEventArgs(string rawArguments)
    {
        RawArguments = rawArguments;
    }

    public string RawArguments { get; }
}
