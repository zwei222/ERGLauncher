namespace ERGLauncher.Core.DialogSettings.Implementations
{
    /// <summary>
    /// Message dialog settings.
    /// </summary>
    public class MessageDialogSettings : ICommonDialogSettings
    {
        /// <inheritdoc />
        public string? InitialDirectory { get; set; } = string.Empty;

        /// <inheritdoc />
        public string? Title { get; set; } = string.Empty;

        /// <summary>
        /// Dialog height.
        /// </summary>
        public double Height { get; set; } = 200;

        /// <summary>
        /// Dialog width.
        /// </summary>
        public double Width { get; set; } = 300;

        /// <summary>
        /// Dialog message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Detail message.
        /// </summary>
        public string? Details { get; set; } = string.Empty;
    }
}
