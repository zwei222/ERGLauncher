using ERGLauncher.Core;

namespace ERGLauncher.Tests.Core;

public sealed class CoreModelTests
{
    [Test]
    public async Task ItemPropertiesRaisePropertyChanged()
    {
        var product = new Product();
        var changedProperties = new List<string>();
        product.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName!);

        product.Name = "Game";
        product.IconPath = "Assets/game.png";
        product.Path = "game.exe";
        product.BrandName = "Brand";

        await Assert.That(changedProperties).IsEquivalentTo([
            nameof(Product.Name),
            nameof(Product.IconPath),
            nameof(Product.Path),
            nameof(Product.BrandName),
        ]);
    }
}
