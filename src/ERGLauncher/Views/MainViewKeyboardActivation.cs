using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using ERGLauncher.Core;

namespace ERGLauncher.Views;

internal static class MainViewKeyboardActivation
{
    internal static bool TryGetActivationItem(
        Key key,
        KeyModifiers modifiers,
        object? source,
        object? focusedElement,
        ListBox mainListBox,
        out Item item)
    {
        item = null!;
        if (key is not (Key.Enter or Key.Space) || modifiers != KeyModifiers.None)
        {
            return false;
        }

        if (TryGetListItem(source, mainListBox, out item) ||
            TryGetListItem(focusedElement, mainListBox, out item))
        {
            return true;
        }

        if (ReferenceEquals(focusedElement, mainListBox) &&
            mainListBox.SelectedItem is Item selectedItem &&
            mainListBox.Items.Contains(selectedItem))
        {
            item = selectedItem;
            return true;
        }

        item = null!;
        return false;
    }

    private static bool TryGetListItem(object? source, ListBox mainListBox, out Item item)
    {
        item = null!;
        if (source is not Visual visual)
        {
            return false;
        }

        var listBoxItem = source as ListBoxItem ??
            visual.FindAncestorOfType<ListBoxItem>(includeSelf: true) ??
            (visual as StyledElement)?.TemplatedParent as ListBoxItem;
        if (listBoxItem?.DataContext is not Item listItem ||
            !IsDescendantOfMainList(listBoxItem, mainListBox))
        {
            return false;
        }

        item = listItem;
        return true;
    }

    private static bool IsDescendantOfMainList(Visual visual, ListBox mainListBox)
    {
        for (Visual? current = visual;
             current is not null;
             current = Avalonia.VisualTree.VisualExtensions.GetVisualParent(current))
        {
            if (ReferenceEquals(current, mainListBox))
            {
                return true;
            }
        }

        return false;
    }
}
