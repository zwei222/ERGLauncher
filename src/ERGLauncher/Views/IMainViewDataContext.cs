using System.Collections.ObjectModel;
using System.Windows.Input;
using ERGLauncher.Core;

namespace ERGLauncher.Views;

/// <summary>
/// Strongly typed binding surface used by <see cref="MainView"/>.
/// </summary>
public interface IMainViewDataContext
{
    string? Title { get; }

    bool IsEnabledBack { get; }

    bool IsEnabledForward { get; }

    string? CurrentBrand { get; }

    Item? SelectedItem { get; set; }

    ObservableCollection<Item> Items { get; }

    ICommand BackCommand { get; }

    ICommand ForwardCommand { get; }

    ICommand SelectItemAsyncCommand { get; }

    ICommand AddItemAsyncCommand { get; }

    ICommand EditItemAsyncCommand { get; }

    ICommand RemoveItemAsyncCommand { get; }

    ICommand OpenSettingCommand { get; }

    ICommand LoadSettingAsyncCommand { get; }

    ICommand SaveAppSettingAsyncCommand { get; }
}
