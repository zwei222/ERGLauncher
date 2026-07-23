using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ERGLauncher.Views;

/// <summary>
/// Product editor view.
/// </summary>
public partial class AddProductView : UserControl
{
    public AddProductView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
