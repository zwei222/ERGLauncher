using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace Avalonia.Controls
{
    public enum WindowState { Normal, Minimized, Maximized, FullScreen }
}

namespace Avalonia.Media.Imaging
{
    public class Bitmap;
}

namespace ERGLauncher.Core
{
    public enum Theme { Light, Dark }

    public abstract class Item
    {
        protected Item(string name, string? iconPath = null) { Name = name; IconPath = iconPath; }
        public string Name { get; }
        public string? IconPath { get; }
    }

    public sealed class RootItem() : Item("Root");
    public sealed class Brand(string name, string? iconPath = null) : Item(name, iconPath);
    public sealed class Product(string name, string path, string? iconPath = null) : Item(name, iconPath)
    {
        public string Path { get; } = path;
    }
}

namespace ERGLauncher.Core.DialogSettings.Implementations
{
    public sealed class OpenFileDialogSettings
    {
        public string Filter { get; set; } = string.Empty;
        public bool CanMultiSelect { get; set; }
        public string Title { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }

    public sealed class MessageDialogSettings;
}

namespace ERGLauncher.Properties
{
    public static class Resources
    {
        public static string ImageFiles => nameof(ImageFiles);
        public static string OpenIconFile => nameof(OpenIconFile);
        public static string ExecutableFiles => nameof(ExecutableFiles);
        public static string OpenExecutableFile => nameof(OpenExecutableFile);
    }
}

namespace ERGLauncher.Models
{
    using Avalonia.Controls;
    using Avalonia.Media.Imaging;
    using ERGLauncher.Core;
    using ERGLauncher.Core.DialogSettings.Implementations;

    public interface IModelBase : INotifyPropertyChanged
    {
        string? Title { get; set; }
        double Height { get; set; }
        double Width { get; set; }
        double Top { get; set; }
        double Left { get; set; }
        WindowState WindowState { get; set; }
        string GetCultureString(string key);
    }

    public interface IMainModel : IModelBase
    {
        bool IsEnabledBack { get; }
        bool IsEnabledForward { get; }
        string? CurrentBrand { get; }
        ObservableCollection<Item> Items { get; }
        Item? SelectedItem { get; set; }
        Item? CurrentItem { get; }
        void Back();
        void Forward();
        ValueTask SelectItemAsync();
        ValueTask AddItemAsync(string name, string? iconFilePath, string filePath);
        ValueTask EditItemAsync(string name, string? iconFilePath, string filePath);
        ValueTask<bool> RemoveItemAsync();
        ValueTask LoadAppSettingAsync();
        ValueTask SaveAppSettingAsync();
        ValueTask LoadSettingAsync();
        ValueTask SaveSettingAsync();
    }

    public interface IAddBrandModel : IModelBase
    {
        string Name { get; set; }
        string? IconPath { get; set; }
        Bitmap? Icon { get; }
        ValueTask SelectIconAsync(string filePath);
        ValueTask<Brand> AddBrandAsync();
        void LoadBrand(Brand brand);
    }

    public interface IAddProductModel : IModelBase
    {
        string Name { get; set; }
        string? IconPath { get; set; }
        Bitmap? Icon { get; }
        string Path { get; set; }
        ValueTask SelectIconAsync(string filePath);
        ValueTask SelectFileAsync(string filePath);
        ValueTask<Product> AddProductAsync();
        void LoadProduct(Product product);
    }

    public interface ISettingModel : IModelBase
    {
        CultureInfo? SelectedLanguage { get; set; }
        Theme SelectedTheme { get; set; }
        ValueTask ApplyAsync();
    }

    public interface IMessageModel : IModelBase
    {
        string Message { get; set; }
        string? Details { get; set; }
        bool IsShowDetails { get; set; }
        void LoadSettings(MessageDialogSettings settings);
    }
}

namespace ERGLauncher.Services
{
    using ERGLauncher.Core.DialogSettings.Implementations;

    public interface ICommonDialogService
    {
        bool ShowDialog(OpenFileDialogSettings settings);
    }
}
