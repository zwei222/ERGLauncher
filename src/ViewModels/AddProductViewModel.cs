using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ERGLauncher.Core;
using ERGLauncher.Core.DialogSettings.Implementations;
using ERGLauncher.Models;
using ERGLauncher.Properties;
using ERGLauncher.Services;

namespace ERGLauncher.ViewModels;

public partial class AddProductViewModel : DialogViewModelBase
{
    private readonly IAddProductModel model;
    private readonly ICommonDialogService commonDialogService;

    public AddProductViewModel(IAddProductModel model, ICommonDialogService commonDialogService)
        : base(model ?? throw new ArgumentNullException(nameof(model)))
    {
        this.model = model;
        this.commonDialogService = commonDialogService ?? throw new ArgumentNullException(nameof(commonDialogService));
        name = model.Name;
        iconPath = model.IconPath;
        icon = model.Icon;
        path = model.Path;
        model.PropertyChanged += OnModelPropertyChanged;
        SelectIconAsyncCommand = new AsyncRelayCommand(SelectIconAsync, () => !IsBusy);
        SelectFileAsyncCommand = new AsyncRelayCommand(SelectFileAsync, () => !IsBusy);
        AddProductAsyncCommand = new AsyncRelayCommand(AddProductAsync, CanAddProduct);
        CancelCommand = new RelayCommand(Cancel, () => !IsBusy);
    }

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string? iconPath;

    [ObservableProperty]
    private Bitmap? icon;

    [ObservableProperty]
    private string path;

    public IAsyncRelayCommand SelectIconAsyncCommand { get; }

    public IAsyncRelayCommand SelectFileAsyncCommand { get; }

    public IAsyncRelayCommand AddProductAsyncCommand { get; }

    public IRelayCommand CancelCommand { get; }

    public override void OnDialogOpened(object? parameter)
    {
        if (parameter is Product product)
        {
            model.LoadProduct(product);
            Refresh();
        }
    }

    partial void OnNameChanged(string value)
    {
        model.Name = value;
        AddProductAsyncCommand.NotifyCanExecuteChanged();
    }

    partial void OnPathChanged(string value)
    {
        model.Path = value;
        AddProductAsyncCommand.NotifyCanExecuteChanged();
    }

    partial void OnIconPathChanged(string? value) => model.IconPath = value;

    protected override void OnBusyStateChanged()
    {
        SelectIconAsyncCommand.NotifyCanExecuteChanged();
        SelectFileAsyncCommand.NotifyCanExecuteChanged();
        AddProductAsyncCommand.NotifyCanExecuteChanged();
        CancelCommand.NotifyCanExecuteChanged();
    }

    protected override void DisposeManaged()
    {
        model.PropertyChanged -= OnModelPropertyChanged;
        base.DisposeManaged();
    }

    private bool CanAddProduct() => !IsBusy && !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Path);

    private async Task SelectIconAsync()
    {
        using var busy = BeginBusy();
        var settings = new OpenFileDialogSettings
        {
            Filter = model.GetCultureString(nameof(Resources.ImageFiles)),
            CanMultiSelect = false,
            Title = model.GetCultureString(nameof(Resources.OpenIconFile)),
        };

        if (commonDialogService.ShowDialog(settings))
        {
            await model.SelectIconAsync(settings.FileName).ConfigureAwait(true);
        }
    }

    private async Task SelectFileAsync()
    {
        using var busy = BeginBusy();
        var settings = new OpenFileDialogSettings
        {
            Filter = model.GetCultureString(nameof(Resources.ExecutableFiles)),
            CanMultiSelect = false,
            Title = model.GetCultureString(nameof(Resources.OpenExecutableFile)),
        };

        if (commonDialogService.ShowDialog(settings))
        {
            await model.SelectFileAsync(settings.FileName).ConfigureAwait(true);
        }
    }

    private async Task AddProductAsync()
    {
        using var busy = BeginBusy();
        RaiseRequestClose(true, await model.AddProductAsync().ConfigureAwait(true));
    }

    private void Cancel() => RaiseRequestClose(false);

    private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IAddProductModel.Name): Name = model.Name; break;
            case nameof(IAddProductModel.IconPath): IconPath = model.IconPath; break;
            case nameof(IAddProductModel.Icon): Icon = model.Icon; break;
            case nameof(IAddProductModel.Path): Path = model.Path; break;
            case null:
            case "": Refresh(); break;
        }
    }

    private void Refresh()
    {
        Name = model.Name;
        IconPath = model.IconPath;
        Icon = model.Icon;
        Path = model.Path;
    }
}
