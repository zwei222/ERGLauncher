using System.Collections.Generic;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ERGLauncher.Views;

/// <summary>
/// Application settings view.
/// </summary>
public partial class SettingView : UserControl
{
    public static IReadOnlyList<CultureInfo> AvailableLanguages { get; } =
    [
        CultureInfo.GetCultureInfo("en-US"),
        CultureInfo.GetCultureInfo("ja-JP"),
    ];

    public static IReadOnlyList<ThemeChoice> AvailableThemeChoices { get; } =
    [
        new(ERGLauncher.Core.Theme.Sync, Properties.Resources.SyncTheme),
        new(ERGLauncher.Core.Theme.Light, Properties.Resources.LightTheme),
        new(ERGLauncher.Core.Theme.Dark, Properties.Resources.DarkTheme),
    ];

    public sealed record ThemeChoice(ERGLauncher.Core.Theme Value, string DisplayName);

    public SettingView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
