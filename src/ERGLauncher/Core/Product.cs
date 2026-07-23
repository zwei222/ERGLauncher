namespace ERGLauncher.Core;

/// <summary>
/// Launchable product.
/// </summary>
public sealed class Product : Item
{
    private string path = string.Empty;
    private string brandName = string.Empty;

    /// <summary>
    /// Gets or sets the executable file path.
    /// </summary>
    public string Path
    {
        get => path;
        set => SetProperty(ref path, value);
    }

    /// <summary>
    /// Gets or sets the owning brand name.
    /// </summary>
    public string BrandName
    {
        get => brandName;
        set => SetProperty(ref brandName, value);
    }
}
