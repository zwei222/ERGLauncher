using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ERGLauncher.Services
{
    /// <summary>
    /// File service interface.
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// Create a BitmapImage asynchronously.
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <returns>BitmapImage</returns>
        public ValueTask<BitmapImage> CreateBitmapImageAsync(string filePath);

        /// <summary>
        /// Copy the icon file asynchronously.
        /// </summary>
        /// <param name="filePath">Icon file path</param>
        /// <returns>Icon file path after copying</returns>
        public ValueTask<string?> CopyIconFileAsync(string? filePath);

        /// <summary>
        /// Run the external process asynchronously.
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <returns>Task</returns>
        public ValueTask ExecuteAsync(string filePath);

        /// <summary>
        /// Save a Bitmap asynchronously.
        /// </summary>
        /// <param name="bitmap">Bitmap</param>
        /// <returns>Bitmap file path</returns>
        public ValueTask<string?> SaveBitmapAsync(Bitmap bitmap);

        /// <summary>
        /// Get the base directory path.
        /// </summary>
        /// <returns>The base directory path</returns>
        public string GetBaseDirectoryPath();

        /// <summary>
        /// Get the default icon file path.
        /// </summary>
        /// <returns>The default icon file path</returns>
        public string GetDefaultIconFilePath();

        /// <summary>
        /// Get the settings directory path.
        /// </summary>
        /// <returns>The settings directory path</returns>
        public string GetSettingsDirectoryPath();
    }
}
