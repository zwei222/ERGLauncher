namespace ERGLauncher.Core.Models;

public sealed class Product : Item
{
    private string path = string.Empty;
    private string brandName = string.Empty;

    public string Path
    {
        get => this.path;
        set => this.SetProperty(ref this.path, value);
    }

    public string BrandName
    {
        get => this.brandName;
        set => this.SetProperty(ref this.brandName, value);
    }
}
