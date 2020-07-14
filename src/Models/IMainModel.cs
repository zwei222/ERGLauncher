using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ERGLauncher.Core;
using MahApps.Metro.Controls.Dialogs;

namespace ERGLauncher.Models
{
    /// <summary>
    /// Main model interface.
    /// </summary>
    public interface IMainModel : IModelBase
    {
        /// <summary>
        /// Dialog coordinator.
        /// </summary>
        public IDialogCoordinator DialogCoordinator { get; }

        /// <summary>
        /// <see langword="true" /> if you can go back; otherwise <see langword="false" />.
        /// </summary>
        public bool IsEnabledBack { get; }

        /// <summary>
        /// <see langword="true" /> if you can go forward; otherwise <see langword="false" />.
        /// </summary>
        public bool IsEnabledForward { get; }

        /// <summary>
        /// Current brand.
        /// </summary>
        public string? CurrentBrand { get; }

        /// <summary>
        /// Display items.
        /// </summary>
        public ObservableCollection<Item> Items { get; }

        /// <summary>
        /// Selected item.
        /// </summary>
        public Item? SelectedItem { get; set; }

        /// <summary>
        /// Current item.
        /// </summary>
        public Item? CurrentItem { get; }

        /// <summary>
        /// Go back history.
        /// </summary>
        public void Back();

        /// <summary>
        /// Go forward history.
        /// </summary>
        public void Forward();

        /// <summary>
        /// Select the item asynchronously.
        /// </summary>
        /// <returns>Task</returns>
        public ValueTask SelectItemAsync();

        /// <summary>
        /// Add items asynchronously.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="iconFilePath">Icon file path</param>
        /// <param name="filePath">executable file Path</param>
        /// <returns>Task</returns>
        public ValueTask AddItemAsync(string name, string? iconFilePath, string filePath);

        /// <summary>
        /// Edit items asynchronously.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="iconFilePath">Icon file path</param>
        /// <param name="filePath">executable file Path</param>
        /// <returns>Task</returns>
        public ValueTask EditItemAsync(string name, string? iconFilePath, string filePath);

        /// <summary>
        /// Remove items asynchronously.
        /// </summary>
        /// <returns><see langword="true" /> if the item was deleted; otherwise <see langword="false" /></returns>
        public ValueTask<bool> RemoveItemAsync();

        /// <summary>
        /// Load the application settings asynchronously.
        /// </summary>
        /// <returns>Task</returns>
        public ValueTask LoadAppSettingAsync();

        /// <summary>
        /// Save the application settings asynchronously.
        /// </summary>
        /// <returns>Task</returns>
        public ValueTask SaveAppSettingAsync();

        /// <summary>
        /// Load the game settings asynchronously.
        /// </summary>
        /// <returns>Task</returns>
        public ValueTask LoadSettingAsync();

        /// <summary>
        /// Save the game settings asynchronously.
        /// </summary>
        /// <returns>Task</returns>
        public ValueTask SaveSettingAsync();
    }
}
