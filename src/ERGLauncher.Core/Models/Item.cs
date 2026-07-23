using System.Text.Json.Serialization;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ERGLauncher.Core.Models;

public abstract class Item : ObservableObject
{
    private Bitmap? icon;
    private string name = string.Empty;
    private string? iconPath;

    [JsonIgnore]
    public Bitmap? Icon
    {
        get => this.icon;
        set => this.SetProperty(ref this.icon, value);
    }

    public string Name
    {
        get => this.name;
        set => this.SetProperty(ref this.name, value);
    }

    public string? IconPath
    {
        get => this.iconPath;
        set => this.SetProperty(ref this.iconPath, value);
    }
}
