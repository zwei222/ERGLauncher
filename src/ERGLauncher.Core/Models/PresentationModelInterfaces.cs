using System.ComponentModel;
using System.Globalization;
using Avalonia.Media.Imaging;

namespace ERGLauncher.Core.Models;

public interface IModelBase : INotifyPropertyChanged
{
    string? Title { get; set; }
    double Height { get; set; }
    double Width { get; set; }
    double Top { get; set; }
    double Left { get; set; }
    CultureInfo CurrentCulture { get; }
    Theme CurrentTheme { get; }
    string? GetCultureString(string key);
}

public interface ISettingModel : IModelBase
{
    CultureInfo? SelectedLanguage { get; set; }
    Theme SelectedTheme { get; set; }
    ValueTask ApplyAsync(CancellationToken cancellationToken = default);
}

public interface IAddBrandModel : IModelBase
{
    string Name { get; set; }
    string? IconPath { get; set; }
    Bitmap? Icon { get; }
    ValueTask SelectIconAsync(string filePath, CancellationToken cancellationToken = default);
    ValueTask<Brand> AddBrandAsync(CancellationToken cancellationToken = default);
    void LoadBrand(Brand brand);
}

public interface IAddProductModel : IModelBase
{
    string Name { get; set; }
    string? IconPath { get; set; }
    Bitmap? Icon { get; }
    string Path { get; set; }
    ValueTask SelectIconAsync(string filePath, CancellationToken cancellationToken = default);
    ValueTask SelectFileAsync(string filePath, CancellationToken cancellationToken = default);
    ValueTask<Product> AddProductAsync(CancellationToken cancellationToken = default);
    void LoadProduct(Product product);
}

public interface IMainModel : IModelBase
{
    bool IsEnabledBack { get; }
    bool IsEnabledForward { get; }
    string? CurrentBrand { get; }
    System.Collections.ObjectModel.ObservableCollection<Item> Items { get; }
    Item? SelectedItem { get; set; }
    Item? CurrentItem { get; }
    void Back();
    void Forward();
    ValueTask SelectItemAsync(CancellationToken cancellationToken = default);
    ValueTask AddItemAsync(string name, string? iconFilePath, string filePath, CancellationToken cancellationToken = default);
    ValueTask EditItemAsync(string name, string? iconFilePath, string filePath, CancellationToken cancellationToken = default);
    ValueTask<bool> RemoveItemAsync(CancellationToken cancellationToken = default);
    ValueTask LoadAppSettingAsync(CancellationToken cancellationToken = default);
    ValueTask SaveAppSettingAsync(CancellationToken cancellationToken = default);
    ValueTask LoadSettingAsync(CancellationToken cancellationToken = default);
    ValueTask SaveSettingAsync(CancellationToken cancellationToken = default);
}

public interface IMessageModel : IModelBase
{
    string Message { get; set; }
    string? Details { get; set; }
    bool IsShowDetails { get; set; }
    void LoadSettings(MessageDialogSettings settings);
}
