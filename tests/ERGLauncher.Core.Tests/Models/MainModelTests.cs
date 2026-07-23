using ERGLauncher.Core.Models;
using ERGLauncher.Core.Services;

namespace ERGLauncher.Core.Tests.Models;

public sealed class MainModelTests
{
    [Test]
    public async Task Main_flow_loads_navigates_adds_edits_removes_and_saves()
    {
        using var directory = new TemporaryDirectory();
        var fileService = new FileService(directory.Path);
        var gameSettings = new GameSettingService(fileService);
        var dialogs = new RecordingDialogService();
        var model = CreateModel(fileService, gameSettings, dialogs);

        await model.LoadSettingAsync();
        await model.AddItemAsync("Studio", null, string.Empty);
        model.SelectedItem = model.Items.Single();
        await model.SelectItemAsync();
        await model.AddItemAsync("Title", null, "/bin/true");
        model.SelectedItem = model.Items.Single();
        await model.EditItemAsync("Renamed", null, "/bin/true");
        var removed = await model.RemoveItemAsync();
        await model.SaveSettingAsync();

        var saved = await gameSettings.LoadSettingAsync();
        await Assert.That(removed).IsTrue();
        await Assert.That(model.Items).IsEmpty();
        await Assert.That(saved.Brands.Single().Products).IsEmpty();
    }

    [Test]
    public async Task Load_and_select_brand_navigates_to_its_products_without_UI_dependencies()
    {
        using var directory = new TemporaryDirectory();
        var fileService = new FileService(directory.Path);
        var gameSettings = new GameSettingService(fileService);
        await gameSettings.SaveSettingAsync(new RootItem([
            new Brand([
                new Product { Name = "Title", BrandName = "Studio", Path = "/bin/true" },
            ]) { Name = "Studio" },
        ]));
        var model = CreateModel(fileService, gameSettings, new RecordingDialogService());

        await model.LoadSettingAsync();
        model.SelectedItem = model.Items.Single();
        await model.SelectItemAsync();

        await Assert.That(model.CurrentBrand).IsEqualTo("Studio");
        await Assert.That(model.Items.Single().Name).IsEqualTo("Title");
        await Assert.That(model.IsEnabledBack).IsTrue();
    }

    [Test]
    public async Task Selecting_product_executes_only_after_confirmation()
    {
        using var directory = new TemporaryDirectory();
        var fileService = new FileService(directory.Path);
        var gameSettings = new GameSettingService(fileService);
        await gameSettings.SaveSettingAsync(new RootItem([
            new Brand([new Product { Name = "Title", BrandName = "Studio", Path = "/path/that/must/not/run" }]) { Name = "Studio" },
        ]));
        var dialogs = new RecordingDialogService { ConfirmationResult = false };
        var model = CreateModel(fileService, gameSettings, dialogs);
        await model.LoadSettingAsync();
        model.SelectedItem = model.Items.Single();
        await model.SelectItemAsync();
        model.SelectedItem = model.Items.Single();

        await model.SelectItemAsync();

        await Assert.That(dialogs.Confirmations).HasSingleItem();
        await Assert.That(dialogs.Confirmations[0].Title).IsEqualTo("Title");
    }

    [Test]
    public async Task Adding_duplicate_brand_shows_already_exists_and_does_not_save_duplicate()
    {
        using var directory = new TemporaryDirectory();
        var fileService = new FileService(directory.Path);
        var gameSettings = new GameSettingService(fileService);
        var dialogs = new RecordingDialogService();
        var model = CreateModel(fileService, gameSettings, dialogs);
        await model.LoadSettingAsync();
        await model.AddItemAsync("Studio", null, string.Empty);

        await model.AddItemAsync("Studio", null, string.Empty);

        await Assert.That(model.Items.Count).IsEqualTo(1);
        await Assert.That(dialogs.Messages).HasSingleItem();
        await Assert.That(dialogs.Messages[0].Message).Contains("Studio");
    }

    [Test]
    public async Task Editing_to_duplicate_name_shows_already_exists_and_preserves_both_items()
    {
        using var directory = new TemporaryDirectory();
        var fileService = new FileService(directory.Path);
        var gameSettings = new GameSettingService(fileService);
        var dialogs = new RecordingDialogService();
        var model = CreateModel(fileService, gameSettings, dialogs);
        await model.LoadSettingAsync();
        await model.AddItemAsync("First", null, string.Empty);
        await model.AddItemAsync("Second", null, string.Empty);
        model.SelectedItem = model.Items.Single(item => item.Name == "Second");

        await model.EditItemAsync("First", null, string.Empty);

        await Assert.That(model.Items.Select(item => item.Name)).IsEquivalentTo(["First", "Second"]);
        await Assert.That(dialogs.Messages).HasSingleItem();
    }

    [Test]
    public async Task Remove_cancelled_by_user_preserves_item()
    {
        using var directory = new TemporaryDirectory();
        var fileService = new FileService(directory.Path);
        var gameSettings = new GameSettingService(fileService);
        var dialogs = new RecordingDialogService { ConfirmationResult = false };
        var model = CreateModel(fileService, gameSettings, dialogs);
        await model.LoadSettingAsync();
        await model.AddItemAsync("Studio", null, string.Empty);
        model.SelectedItem = model.Items.Single();

        var removed = await model.RemoveItemAsync();

        await Assert.That(removed).IsFalse();
        await Assert.That(model.Items.Count).IsEqualTo(1);
    }

    private static MainModel CreateModel(
        IFileService fileService,
        IGameSettingService gameSettings,
        IDialogService dialogs) =>
        new(
            fileService,
            new AppSettingService(fileService),
            gameSettings,
            new ResourceService(),
            new ThemeService(),
            dialogs);

    private sealed class RecordingDialogService : IDialogService
    {
        public bool ConfirmationResult { get; set; } = true;
        public List<(string Title, string Message)> Confirmations { get; } = [];
        public List<(string Title, string Message)> Messages { get; } = [];

        public ValueTask<bool> ShowConfirmationAsync(
            string title,
            string message,
            CancellationToken cancellationToken = default)
        {
            this.Confirmations.Add((title, message));
            return ValueTask.FromResult(this.ConfirmationResult);
        }

        public ValueTask ShowMessageAsync(
            string title,
            string message,
            CancellationToken cancellationToken = default)
        {
            this.Messages.Add((title, message));
            return ValueTask.CompletedTask;
        }
    }
}
