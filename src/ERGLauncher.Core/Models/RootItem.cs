using System.Text.Json.Serialization;

namespace ERGLauncher.Core.Models;

public sealed class RootItem : Item
{
    [JsonConstructor]
    public RootItem(ICollection<Brand>? brands)
    {
        this.Brands = brands ?? [];
    }

    public ICollection<Brand> Brands { get; }
}
