using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ERGLauncher.Core;
using ERGLauncher.Properties;
using ERGLauncher.Services;

namespace ERGLauncher.ViewModels;

/// <summary>
/// View model for the Add/Edit brand dialog. Produces an <see cref="ERGLauncher.Core.Brand"/>
/// result consumed by <see cref="MainViewModel"/>.
/// </summary>
public sealed partial class AddBrandViewModel : DialogViewModelBase
{
    private readonly IFilePickerService filePickerService;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string? iconPath;

    [ObservableProperty]
    private Bitmap? icon;

    public AddBrandViewModel(IFilePickerService filePickerService)
    {
        this.filePickerService = filePickerService ?? throw new ArgumentNullException(nameof(filePickerService));
        SelectIconAsyncCommand = new AsyncRelayCommand(SelectIconAsync, () => !IsBusy);
        AddBrandAsyncCommand = new AsyncRelayCommand(AddBrandAsync, CanAddBrand);
        CancelCommand = new RelayCommand(Cancel, () => !IsBusy);
    }

    public IAsyncRelayCommand SelectIconAsyncCommand { get; }

    public IAsyncRelayCommand AddBrandAsyncCommand { get; }

    public IRelayCommand CancelCommand { get; }

    public override void OnDialogOpened(object? parameter)
    {
        if (parameter is Brand brand)
        {
            Name = brand.Name;
            IconPath = brand.IconPath;
            Icon = brand.Icon;
        }
    }

    partial void OnNameChanged(string value) => AddBrandAsyncCommand.NotifyCanExecuteChanged();

    protected override void OnBusyStateChanged()
    {
        SelectIconAsyncCommand.NotifyCanExecuteChanged();
        AddBrandAsyncCommand.NotifyCanExecuteChanged();
        CancelCommand.NotifyCanExecuteChanged();
    }

    private bool CanAddBrand() => !IsBusy && !string.IsNullOrWhiteSpace(Name);

    private async Task SelectIconAsync()
    {
        using var busy = BeginBusy();
        var picked = await filePickerService.PickFileAsync(
            Resources.OpenIconFile,
            ["*.png", "*.jpg", "*.jpeg", "*.bmp", "*.ico"]).ConfigureAwait(true);
        if (!string.IsNullOrWhiteSpace(picked))
        {
            IconPath = picked;
            Icon = new Bitmap(picked);
        }
    }

    private Task AddBrandAsync()
    {
        var brand = new Brand([])
        {
            Name = Name,
            IconPath = IconPath,
            Icon = Icon,
        };
        RaiseRequestClose(true, brand);
        return Task.CompletedTask;
    }

    private void Cancel() => RaiseRequestClose(false);
}
