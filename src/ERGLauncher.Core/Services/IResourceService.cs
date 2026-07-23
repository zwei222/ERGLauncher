using System.Globalization;

namespace ERGLauncher.Core.Services;

public interface IResourceService
{
    CultureInfo CurrentCulture { get; }

    void ChangeCulture(CultureInfo? culture);

    string? GetCultureString(string key);
}
