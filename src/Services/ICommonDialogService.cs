using ERGLauncher.Core.DialogSettings;

namespace ERGLauncher.Services
{
    /// <summary>
    /// Common dialog service interface.
    /// </summary>
    public interface ICommonDialogService
    {
        /// <summary>
        /// Show dialog.
        /// </summary>
        /// <param name="settings">Dialog settings</param>
        /// <returns><see langword="true" /> if the dialog is displayed; otherwise <see langword="false"/>.</returns>
        public bool ShowDialog(ICommonDialogSettings settings);
    }
}
