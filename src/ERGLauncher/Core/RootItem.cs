using System.Text.Json.Serialization;

namespace ERGLauncher.Core;

/// <summary>
/// Root of the persisted game tree.
/// </summary>
public sealed class RootItem : Item
{
    /// <summary>
    /// Initializes the root with its brands.
    /// </summary>
    [JsonConstructor]
    public RootItem(ICollection<Brand> brands)
    {
        Brands = brands;
    }

    /// <summary>
    /// Gets the configured brands.
    /// </summary>
    public ICollection<Brand> Brands { get; }
}
