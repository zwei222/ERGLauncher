using System.Globalization;
using System.Runtime.Serialization;
using Utf8Json;

internal static class Program
{
    public static void Main(string[] args)
    {
        var outputDirectory = args.Single();
        Directory.CreateDirectory(outputDirectory);

        var appSettings = new AppSettings
        {
            Culture = CultureInfo.GetCultureInfo("ja-JP"),
            Theme = Theme.Dark,
        };
        var gameSettings = new RootItem([
            new Brand([
                new Product
                {
                    Path = @"C:\Games\Example\game.exe",
                    BrandName = "Example Brand",
                    Name = "Example Game",
                    IconPath = "Assets/example-game.png",
                },
            ])
            {
                Name = "Example Brand",
                IconPath = "Assets/example-brand.png",
            },
        ])
        {
            Name = "Root",
            IconPath = null,
        };

        File.WriteAllBytes(Path.Combine(outputDirectory, "appSettings.json"), JsonSerializer.Serialize(appSettings));
        File.WriteAllBytes(Path.Combine(outputDirectory, "gameSettings.json"), JsonSerializer.Serialize(gameSettings));
    }
}

public sealed class AppSettings
{
    [JsonFormatter(typeof(CultureInfoJsonFormatter))]
    public CultureInfo Culture { get; set; } = CultureInfo.CurrentUICulture;

    [JsonFormatter(typeof(NumericThemeJsonFormatter))]
    public Theme Theme { get; set; }
}

public enum Theme
{
    None = 0,
    Light = 1,
    Dark = 2,
    Sync = 3,
}

public abstract class Item
{
    [IgnoreDataMember]
    public object? Icon { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? IconPath { get; set; }
}

public sealed class RootItem : Item
{
    [SerializationConstructor]
    public RootItem(ICollection<Brand> brands) => this.Brands = brands;

    public ICollection<Brand> Brands { get; }
}

public sealed class Brand : Item
{
    [SerializationConstructor]
    public Brand(ICollection<Product> products) => this.Products = products;

    public ICollection<Product> Products { get; }
}

public sealed class Product : Item
{
    public string Path { get; set; } = string.Empty;

    public string BrandName { get; set; } = string.Empty;
}

public sealed class CultureInfoJsonFormatter : IJsonFormatter<CultureInfo>
{
    public void Serialize(ref JsonWriter writer, CultureInfo value, IJsonFormatterResolver formatterResolver)
    {
        writer.WriteBeginObject();
        writer.WritePropertyName(nameof(value.Name));
        writer.WriteString(value.Name);
        writer.WriteEndObject();
    }

    public CultureInfo Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        throw new NotSupportedException();
    }
}

public sealed class NumericThemeJsonFormatter : IJsonFormatter<Theme>
{
    public void Serialize(ref JsonWriter writer, Theme value, IJsonFormatterResolver formatterResolver)
    {
        writer.WriteInt32((int)value);
    }

    public Theme Deserialize(ref JsonReader reader, IJsonFormatterResolver formatterResolver)
    {
        throw new NotSupportedException();
    }
}
