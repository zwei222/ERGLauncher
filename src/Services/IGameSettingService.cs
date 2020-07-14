using System.Threading;
using System.Threading.Tasks;
using ERGLauncher.Core;

namespace ERGLauncher.Services
{
    /// <summary>
    /// Game setting service interface.
    /// </summary>
    public interface IGameSettingService
    {
        /// <summary>
        /// Load settings asynchronously.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Root item</returns>
        public ValueTask<RootItem> LoadSettingAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Save settings asynchronously.
        /// </summary>
        /// <param name="rootItem">Root item</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public ValueTask SaveSettingAsync(RootItem? rootItem, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create a brand item asynchronously.
        /// </summary>
        /// <param name="name">Brand name</param>
        /// <param name="iconFilePath">Icon file path</param>
        /// <returns>Brand</returns>
        public ValueTask<Brand> CreateBrandItemAsync(string name, string? iconFilePath);

        /// <summary>
        /// Create a product item asynchronously.
        /// </summary>
        /// <param name="name">Product name</param>
        /// <param name="iconFilePath">Icon file path</param>
        /// <param name="brandName">Brand name</param>
        /// <param name="gameFilePath">Game file path</param>
        /// <returns>Product</returns>
        public ValueTask<Product> CreateProductItemAsync(string name, string? iconFilePath, string brandName, string gameFilePath);
    }
}
