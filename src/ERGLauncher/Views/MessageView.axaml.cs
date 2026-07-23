using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ERGLauncher.Views;

/// <summary>
/// Message dialog content view.
/// </summary>
public partial class MessageView : UserControl
{
    public MessageView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
