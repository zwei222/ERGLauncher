using System.Text.Json.Serialization;

namespace ERGLauncher.Core.Models;

public sealed class Brand : Item
{
    [JsonConstructor]
    public Brand(ICollection<Product>? products)
    {
        this.Products = products ?? [];
    }

    public ICollection<Product> Products { get; }
}
