using System.Globalization;

namespace ERGLauncher.Core.Services;

public sealed class ResourceService : IResourceService
{
    private readonly Func<string, CultureInfo, string?> resourceLookup;

    public ResourceService(
        Func<string, CultureInfo, string?>? resourceLookup = null,
        CultureInfo? initialCulture = null)
    {
        this.resourceLookup = resourceLookup ?? ((_, _) => null);
        this.CurrentCulture = initialCulture ?? CultureInfo.CurrentUICulture;
    }

    public CultureInfo CurrentCulture { get; private set; }

    public void ChangeCulture(CultureInfo? culture)
    {
        this.CurrentCulture = culture ?? CultureInfo.CurrentUICulture;
    }

    public string? GetCultureString(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return this.resourceLookup(key, this.CurrentCulture);
    }
}
