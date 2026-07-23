using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ERGLauncher.Core;
using ERGLauncher.Models;
using ERGLauncher.Services;

namespace ERGLauncher.ViewModels.Tests;

public class AddProductViewModelTests
{
    [Test]
    public async Task AddCommand_TracksRequiredFieldsAndWritesChangesToModel()
    {
        var model = new AddProductModelStub();
        using var viewModel = new AddProductViewModel(model, new DialogServiceStub());

        await Assert.That(viewModel.AddProductAsyncCommand.CanExecute(null)).IsFalse();

        viewModel.Name = "Game";
        viewModel.Path = "/games/game";

        await Assert.That(model.Name).IsEqualTo("Game");
        await Assert.That(model.Path).IsEqualTo("/games/game");
        await Assert.That(viewModel.AddProductAsyncCommand.CanExecute(null)).IsTrue();
    }

    [Test]
    public async Task ModelPropertyChange_IsReflectedByViewModel()
    {
        var model = new AddProductModelStub();
        using var viewModel = new AddProductViewModel(model, new DialogServiceStub());

        model.SetName("Updated externally");

        await Assert.That(viewModel.Name).IsEqualTo("Updated externally");
    }

    [Test]
    public async Task AddCommand_RaisesAcceptedCloseResult()
    {
        var product = new Product("Game", "/games/game");
        var model = new AddProductModelStub { Name = product.Name, Path = product.Path, ProductToReturn = product };
        using var viewModel = new AddProductViewModel(model, new DialogServiceStub());
        DialogCloseRequestedEventArgs? close = null;
        viewModel.RequestClose += (_, args) => close = args;

        await viewModel.AddProductAsyncCommand.ExecuteAsync(null);

        await Assert.That(close).IsNotNull();
        await Assert.That(close!.Accepted).IsTrue();
        await Assert.That(close.Value).IsSameReferenceAs(product);
        await Assert.That(viewModel.IsBusy).IsFalse();
    }

    private sealed class DialogServiceStub : ICommonDialogService
    {
        public bool ShowDialog(Core.DialogSettings.Implementations.OpenFileDialogSettings settings) => false;
    }

    private sealed class AddProductModelStub : IAddProductModel
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public string? Title { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
        public double Top { get; set; }
        public double Left { get; set; }
        public WindowState WindowState { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? IconPath { get; set; }
        public Bitmap? Icon { get; private set; }
        public string Path { get; set; } = string.Empty;
        public Product ProductToReturn { get; set; } = new("Product", "/product");

        public string GetCultureString(string key) => key;
        public ValueTask SelectIconAsync(string filePath) => ValueTask.CompletedTask;
        public ValueTask SelectFileAsync(string filePath) => ValueTask.CompletedTask;
        public ValueTask<Product> AddProductAsync() => ValueTask.FromResult(ProductToReturn);

        public void LoadProduct(Product product)
        {
            Name = product.Name;
            IconPath = product.IconPath;
            Path = product.Path;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        public void SetName(string value)
        {
            Name = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        }
    }
}
