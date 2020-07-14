using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ERGLauncher.Core;

namespace ERGLauncher.Models
{
    /// <summary>
    /// Add brand model interface.
    /// </summary>
    public interface IAddBrandModel : IModelBase
    {
        /// <summary>
        /// Brand name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Icon file path.
        /// </summary>
        public string? IconPath { get; set; }

        /// <summary>
        /// Icon.
        /// </summary>
        public BitmapImage? Icon { get; }

        /// <summary>
        /// Select the icon asynchronously.
        /// </summary>
        /// <param name="filePath">Icon file path</param>
        /// <returns>Task</returns>
        public ValueTask SelectIconAsync(string filePath);

        /// <summary>
        /// Add brands asynchronously.
        /// </summary>
        /// <returns>Brand</returns>
        public ValueTask<Brand> AddBrandAsync();

        /// <summary>
        /// Load the brand.
        /// </summary>
        /// <param name="brand">Brand</param>
        public void LoadBrand(Brand brand);
    }
}
