using ERGLauncher.Core.DialogSettings;
using ERGLauncher.Core.DialogSettings.Implementations;

namespace ERGLauncher.Services.Implementations
{
    /// <summary>
    /// Common dialog service.
    /// </summary>
    public class CommonDialogService : ICommonDialogService
    {
        /// <inheritdoc />
        public bool ShowDialog(ICommonDialogSettings settings)
        {
            var service = this.CreateInnerService(settings);

            if (service == null)
            {
                return false;
            }

            return service.ShowDialog(settings);
        }

        /// <summary>
        /// Create inner service.
        /// </summary>
        /// <param name="settings">Dialog settings</param>
        /// <returns>Dialog service</returns>
        private ICommonDialogService? CreateInnerService(ICommonDialogSettings settings)
        {
            if (settings == null)
            {
                return null;
            }

            return settings switch
            {
                OpenFileDialogSettings _ => new FileDialogService(),
                _ => null
            };
        }
    }
}
