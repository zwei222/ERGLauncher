using ERGLauncher.Core.DialogSettings.Implementations;

namespace ERGLauncher.Models
{
    /// <summary>
    /// Message dialog model interface.
    /// </summary>
    public interface IMessageModel : IModelBase
    {
        /// <summary>
        /// Message.
        /// </summary>

        public string Message { get; set; }

        /// <summary>
        /// Detailed message.
        /// </summary>
        public string? Details { get; set; }

        /// <summary>
        /// <see langword="true" /> if show detailed message; otherwise <see langword="false" />
        /// </summary>
        public bool IsShowDetails { get; set; }

        /// <summary>
        /// Load the settings asynchronously.
        /// </summary>
        /// <param name="settings">Message dialog settings</param>
        public void LoadSettings(MessageDialogSettings settings);
    }
}
