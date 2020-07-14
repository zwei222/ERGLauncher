using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ERGLauncher.Core;

namespace ERGLauncher.Models
{
    /// <summary>
    /// Add product model interface.
    /// </summary>
    public interface IAddProductModel : IModelBase
    {
        /// <summary>
        /// Product name.
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
        /// Executable file path.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Select the icon asynchronously.
        /// </summary>
        /// <param name="filePath">Icon file path</param>
        /// <returns>Task</returns>
        public ValueTask SelectIconAsync(string filePath);

        /// <summary>
        /// Select the executable file asynchronously.
        /// </summary>
        /// <param name="filePath">Icon file path</param>
        /// <returns>Task</returns>
        public ValueTask SelectFileAsync(string filePath);

        /// <summary>
        /// Add products asynchronously.
        /// </summary>
        /// <returns>Product</returns>
        public ValueTask<Product> AddProductAsync();

        /// <summary>
        /// Load the product.
        /// </summary>
        /// <param name="product">Product</param>
        public void LoadProduct(Product product);
    }
}
