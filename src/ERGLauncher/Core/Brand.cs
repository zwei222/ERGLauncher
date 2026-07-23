using System.Text.Json.Serialization;

namespace ERGLauncher.Core;

/// <summary>
/// Game brand.
/// </summary>
public sealed class Brand : Item
{
    /// <summary>
    /// Initializes a brand with its products.
    /// </summary>
    [JsonConstructor]
    public Brand(ICollection<Product> products)
    {
        Products = products;
    }

    /// <summary>
    /// Gets the products belonging to this brand.
    /// </summary>
    public ICollection<Product> Products { get; }
}
