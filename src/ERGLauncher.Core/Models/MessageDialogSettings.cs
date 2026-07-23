namespace ERGLauncher.Core.Models;

public sealed class MessageDialogSettings
{
    public string? Title { get; init; }
    public double Height { get; init; }
    public double Width { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? Details { get; init; }
}
