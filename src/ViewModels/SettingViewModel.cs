using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ERGLauncher.Core;
using ERGLauncher.Models;

namespace ERGLauncher.ViewModels;

public partial class SettingViewModel : DialogViewModelBase
{
    private readonly ISettingModel model;

    public SettingViewModel(ISettingModel model)
        : base(model ?? throw new ArgumentNullException(nameof(model)))
    {
        this.model = model;
        selectedLanguage = model.SelectedLanguage;
        selectedTheme = model.SelectedTheme;
        model.PropertyChanged += OnModelPropertyChanged;
        OkAsyncCommand = new AsyncRelayCommand(OkAsync, () => !IsBusy);
        ApplyAsyncCommand = new AsyncRelayCommand(ApplyAsync, () => !IsBusy);
    }

    [ObservableProperty]
    private CultureInfo? selectedLanguage;

    [ObservableProperty]
    private Theme selectedTheme;

    public IAsyncRelayCommand OkAsyncCommand { get; }

    public IAsyncRelayCommand ApplyAsyncCommand { get; }

    partial void OnSelectedLanguageChanged(CultureInfo? value) => model.SelectedLanguage = value;

    partial void OnSelectedThemeChanged(Theme value) => model.SelectedTheme = value;

    protected override void OnBusyStateChanged()
    {
        OkAsyncCommand.NotifyCanExecuteChanged();
        ApplyAsyncCommand.NotifyCanExecuteChanged();
    }

    protected override void DisposeManaged()
    {
        model.PropertyChanged -= OnModelPropertyChanged;
        base.DisposeManaged();
    }

    private async Task OkAsync()
    {
        await ApplyAsync().ConfigureAwait(true);
        RaiseRequestClose(true);
    }

    private async Task ApplyAsync()
    {
        using var busy = BeginBusy();
        await model.ApplyAsync().ConfigureAwait(true);
    }

    private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ISettingModel.SelectedLanguage): SelectedLanguage = model.SelectedLanguage; break;
            case nameof(ISettingModel.SelectedTheme): SelectedTheme = model.SelectedTheme; break;
            case null:
            case "":
                SelectedLanguage = model.SelectedLanguage;
                SelectedTheme = model.SelectedTheme;
                break;
        }
    }
}
