extern alias MigratedCore;

using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ERGLauncher.Core;
using ERGLauncher.Properties;
using ERGLauncher.Services;
using IFileService = MigratedCore::ERGLauncher.Core.Services.IFileService;

namespace ERGLauncher.ViewModels;

/// <summary>
/// View model for the Add/Edit product dialog. Produces an <see cref="ERGLauncher.Core.Product"/>
/// result consumed by <see cref="MainViewModel"/>.
/// </summary>
public sealed partial class AddProductViewModel : DialogViewModelBase
{
    private readonly IFilePickerService filePickerService;
    private readonly IFileService fileService;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string? iconPath;

    [ObservableProperty]
    private Bitmap? icon;

    [ObservableProperty]
    private string path = string.Empty;

    public AddProductViewModel(IFilePickerService filePickerService, IFileService fileService)
    {
        this.filePickerService = filePickerService ?? throw new ArgumentNullException(nameof(filePickerService));
        this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        SelectIconAsyncCommand = new AsyncRelayCommand(SelectIconAsync, () => !IsBusy);
        SelectFileAsyncCommand = new AsyncRelayCommand(SelectFileAsync, () => !IsBusy);
        AddProductAsyncCommand = new AsyncRelayCommand(AddProductAsync, CanAddProduct);
        CancelCommand = new RelayCommand(Cancel, () => !IsBusy);
    }

    public IAsyncRelayCommand SelectIconAsyncCommand { get; }

    public IAsyncRelayCommand SelectFileAsyncCommand { get; }

    public IAsyncRelayCommand AddProductAsyncCommand { get; }

    public IRelayCommand CancelCommand { get; }

    public override void OnDialogOpened(object? parameter)
    {
        if (parameter is Product product)
        {
            Name = product.Name;
            IconPath = product.IconPath;
            Icon = product.Icon;
            Path = product.Path;
        }
    }

    partial void OnNameChanged(string value) => AddProductAsyncCommand.NotifyCanExecuteChanged();

    partial void OnPathChanged(string value) => AddProductAsyncCommand.NotifyCanExecuteChanged();

    protected override void OnBusyStateChanged()
    {
        SelectIconAsyncCommand.NotifyCanExecuteChanged();
        SelectFileAsyncCommand.NotifyCanExecuteChanged();
        AddProductAsyncCommand.NotifyCanExecuteChanged();
        CancelCommand.NotifyCanExecuteChanged();
    }

    private bool CanAddProduct() => !IsBusy && !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Path);

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

    private async Task SelectFileAsync()
    {
        using var busy = BeginBusy();
        var picked = await filePickerService.PickFileAsync(
            Resources.OpenExecutableFile,
            ["*.exe", "*"]).ConfigureAwait(true);
        if (!string.IsNullOrWhiteSpace(picked))
        {
            Path = picked;
            if (string.IsNullOrWhiteSpace(IconPath))
            {
                IconPath = await fileService.ExtractAssociatedIconAsync(picked).ConfigureAwait(true);
                if (!string.IsNullOrWhiteSpace(IconPath))
                {
                    Icon = await fileService.CreateBitmapAsync(IconPath).ConfigureAwait(true);
                }
            }
        }
    }

    private Task AddProductAsync()
    {
        var product = new Product
        {
            Name = Name,
            IconPath = IconPath,
            Icon = Icon,
            Path = Path,
        };
        RaiseRequestClose(true, product);
        return Task.CompletedTask;
    }

    private void Cancel() => RaiseRequestClose(false);
}
