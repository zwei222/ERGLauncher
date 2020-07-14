namespace ERGLauncher.Core.DialogSettings
{
    /// <summary>
    /// Dialog settings interface.
    /// </summary>
    public interface ICommonDialogSettings
    {
        /// <summary>
        /// Initial directory path.
        /// </summary>
        public string? InitialDirectory { get; set; }

        /// <summary>
        /// Title.
        /// </summary>
        public string? Title { get; set; }
    }
}
