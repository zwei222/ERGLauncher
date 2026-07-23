namespace ERGLauncher.ViewModels.Tests;

public class DependencyTests
{
    [Test]
    public async Task ViewModels_DoNotReferenceReactivePropertyOrPrism()
    {
        var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        var viewModelsDirectory = Path.Combine(repositoryRoot, "src/ViewModels");
        var files = Directory.GetFiles(viewModelsDirectory, "*.cs");

        foreach (var file in files)
        {
            var source = await File.ReadAllTextAsync(file);
            await Assert.That(source).DoesNotContain("Reactive.Bindings");
            await Assert.That(source).DoesNotContain("System.Reactive");
            await Assert.That(source).DoesNotContain("Prism.");
        }

        var project = await File.ReadAllTextAsync(Path.Combine(repositoryRoot, "src/ERGLauncher.csproj"));
        await Assert.That(project).Contains("CommunityToolkit.Mvvm");
        await Assert.That(project).DoesNotContain("PackageReference Include=\"ReactiveProperty\"");
    }
}
