using Avalonia.Media.Imaging;
using ERGLauncher.Core.Services;
using ERGLauncher.Services;
using ERGLauncher.ViewModels;
using NSubstitute;
using System.Runtime.CompilerServices;

namespace ERGLauncher.Tests.ViewModels;

public sealed class AddProductViewModelTests
{
    [Test]
    public async Task SelectFileAsync_extracts_and_displays_associated_icon_when_no_custom_icon_exists()
    {
        using var directory = new TemporaryDirectory();
        var executablePath = Path.Combine(directory.Path, "game.exe");
        var iconPath = Path.Combine(directory.Path, "game.png");
        await File.WriteAllTextAsync(executablePath, string.Empty);

        var picker = Substitute.For<IFilePickerService>();
        picker.PickFileAsync(Arg.Any<string?>(), Arg.Any<string[]?>()).Returns(executablePath);
        var fileService = Substitute.For<IFileService>();
        fileService.ExtractAssociatedIconAsync(executablePath, Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult<string?>(iconPath));
        var extractedBitmap = (Bitmap)RuntimeHelpers.GetUninitializedObject(typeof(Bitmap));
        fileService.CreateBitmapAsync(iconPath, Arg.Any<CancellationToken>())
            .Returns(ValueTask.FromResult<Bitmap?>(extractedBitmap));
        var viewModel = new AddProductViewModel(picker, fileService);

        await viewModel.SelectFileAsyncCommand.ExecuteAsync(null);

        await fileService.Received(1).ExtractAssociatedIconAsync(executablePath, Arg.Any<CancellationToken>());
        await fileService.Received(1).CreateBitmapAsync(iconPath, Arg.Any<CancellationToken>());
        await Assert.That(viewModel.IconPath).IsEqualTo(iconPath);
        await Assert.That(viewModel.Icon).IsSameReferenceAs(extractedBitmap);
    }

    [Test]
    public async Task SelectFileAsync_preserves_custom_icon_without_extracting_associated_icon()
    {
        using var directory = new TemporaryDirectory();
        var executablePath = Path.Combine(directory.Path, "game.exe");
        await File.WriteAllTextAsync(executablePath, string.Empty);
        const string customIconPath = "/icons/custom.png";

        var picker = Substitute.For<IFilePickerService>();
        picker.PickFileAsync(Arg.Any<string?>(), Arg.Any<string[]?>()).Returns(executablePath);
        var fileService = Substitute.For<IFileService>();
        var viewModel = new AddProductViewModel(picker, fileService)
        {
            IconPath = customIconPath,
        };

        await viewModel.SelectFileAsyncCommand.ExecuteAsync(null);

        await fileService.DidNotReceive().ExtractAssociatedIconAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await Assert.That(viewModel.IconPath).IsEqualTo(customIconPath);
    }
}
