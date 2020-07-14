using ERGLauncher.Core.DialogSettings;
using ERGLauncher.Core.DialogSettings.Implementations;
using Microsoft.Win32;

namespace ERGLauncher.Services.Implementations
{
    /// <summary>
    /// File dialog service.
    /// </summary>
    public class FileDialogService : ICommonDialogService
    {
        /// <inheritdoc />
        public bool ShowDialog(ICommonDialogSettings settings)
        {
            var dialog = this.CreateDialogService(settings);

            if (dialog == null)
            {
                return false;
            }

            var returnValue = dialog.ShowDialog();

            if (returnValue.HasValue)
            {
                this.SetReturnValue(dialog, settings);

                return returnValue.Value;
            }

            return false;
        }

        /// <summary>
        /// Create a dialog service.
        /// </summary>
        /// <param name="settings">Dialog settings</param>
        /// <returns>File dialog</returns>
        private FileDialog? CreateDialogService(ICommonDialogSettings settings)
        {
            if (settings == null)
            {
                return null;
            }

            if (!(settings is OpenFileDialogSettings openFileDialogSettings))
            {
                return null;
            }

            FileDialog? fileDialog = new OpenFileDialog
            {
                Filter = openFileDialogSettings.Filter,
                FilterIndex = openFileDialogSettings.FilterIndex,
                InitialDirectory = openFileDialogSettings.InitialDirectory,
                Title = openFileDialogSettings.Title,
            };

            return fileDialog;
        }

        /// <summary>
        /// Set the return value.
        /// </summary>
        /// <param name="fileDialog">File dialog</param>
        /// <param name="settings">Dialog settings</param>
        private void SetReturnValue(FileDialog fileDialog, ICommonDialogSettings settings)
        {
            switch (settings)
            {
                case OpenFileDialogSettings openFileDialogSettings:
                    var openFileDialog = (OpenFileDialog)fileDialog;

                    openFileDialogSettings.FileName = openFileDialog.FileName;

                    foreach (var fileName in openFileDialog.FileNames)
                    {
                        openFileDialogSettings.FileNames.Add(fileName);
                    }

                    break;
            }
        }
    }
}
