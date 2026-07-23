using System.Text.Json.Serialization;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ERGLauncher.Core;

/// <summary>
/// Base item displayed by the launcher.
/// </summary>
public abstract class Item : ObservableObject
{
    private Bitmap? icon;
    private string name = string.Empty;
    private string? iconPath;

    /// <summary>
    /// Gets or sets the runtime icon. Icons are loaded separately from settings JSON.
    /// </summary>
    [JsonIgnore]
    public Bitmap? Icon
    {
        get => icon;
        set => SetProperty(ref icon, value);
    }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }

    /// <summary>
    /// Gets or sets the icon file path.
    /// </summary>
    public string? IconPath
    {
        get => iconPath;
        set => SetProperty(ref iconPath, value);
    }
}
