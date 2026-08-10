using Avalonia.Controls;

namespace ERGLauncher.Services;

internal static class AvaloniaDialogButtonFactory
{
    public static IReadOnlyList<Button> CreateActionButtons(bool showCancel)
    {
        var buttons = new List<Button>();
        if (showCancel)
        {
            buttons.Add(CreateButton(Properties.Resources.Cancel, isCancel: true, isAffirmative: false));
        }

        buttons.Add(CreateButton(Properties.Resources.Ok, isCancel: false, isAffirmative: true));
        return buttons;
    }

    private static Button CreateButton(string content, bool isCancel, bool isAffirmative)
    {
        var button = new Button
        {
            Content = content,
            MinWidth = 90,
            IsDefault = isAffirmative,
            IsCancel = isCancel,
        };
        button.Classes.Add("dialog-control");
        if (isAffirmative)
        {
            button.Classes.Add("accent");
        }

        return button;
    }
}
