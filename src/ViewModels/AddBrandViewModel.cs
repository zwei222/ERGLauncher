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

public partial class AddBrandViewModel : DialogViewModelBase
{
    private readonly IAddBrandModel model;
    private readonly ICommonDialogService commonDialogService;

    public AddBrandViewModel(IAddBrandModel model, ICommonDialogService commonDialogService)
        : base(model ?? throw new ArgumentNullException(nameof(model)))
    {
        this.model = model;
        this.commonDialogService = commonDialogService ?? throw new ArgumentNullException(nameof(commonDialogService));
        name = model.Name;
        iconPath = model.IconPath;
        icon = model.Icon;
        model.PropertyChanged += OnModelPropertyChanged;
        SelectIconAsyncCommand = new AsyncRelayCommand(SelectIconAsync, () => !IsBusy);
        AddBrandAsyncCommand = new AsyncRelayCommand(AddBrandAsync, CanAddBrand);
        CancelCommand = new RelayCommand(Cancel, () => !IsBusy);
    }

    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string? iconPath;

    [ObservableProperty]
    private Bitmap? icon;

    public IAsyncRelayCommand SelectIconAsyncCommand { get; }

    public IAsyncRelayCommand AddBrandAsyncCommand { get; }

    public IRelayCommand CancelCommand { get; }

    public override void OnDialogOpened(object? parameter)
    {
        if (parameter is Brand brand)
        {
            model.LoadBrand(brand);
            Refresh();
        }
    }

    partial void OnNameChanged(string value)
    {
        model.Name = value;
        AddBrandAsyncCommand.NotifyCanExecuteChanged();
    }

    partial void OnIconPathChanged(string? value) => model.IconPath = value;

    protected override void OnBusyStateChanged()
    {
        SelectIconAsyncCommand.NotifyCanExecuteChanged();
        AddBrandAsyncCommand.NotifyCanExecuteChanged();
        CancelCommand.NotifyCanExecuteChanged();
    }

    protected override void DisposeManaged()
    {
        model.PropertyChanged -= OnModelPropertyChanged;
        base.DisposeManaged();
    }

    private bool CanAddBrand() => !IsBusy && !string.IsNullOrWhiteSpace(Name);

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

    private async Task AddBrandAsync()
    {
        using var busy = BeginBusy();
        RaiseRequestClose(true, await model.AddBrandAsync().ConfigureAwait(true));
    }

    private void Cancel() => RaiseRequestClose(false);

    private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IAddBrandModel.Name): Name = model.Name; break;
            case nameof(IAddBrandModel.IconPath): IconPath = model.IconPath; break;
            case nameof(IAddBrandModel.Icon): Icon = model.Icon; break;
            case null:
            case "": Refresh(); break;
        }
    }

    private void Refresh()
    {
        Name = model.Name;
        IconPath = model.IconPath;
        Icon = model.Icon;
    }
}
