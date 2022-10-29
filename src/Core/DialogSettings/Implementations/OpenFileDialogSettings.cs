using System.Collections.Generic;

namespace ERGLauncher.Core.DialogSettings.Implementations
{
    /// <summary>
    /// Open file dialog settings.
    /// </summary>
    public class OpenFileDialogSettings : ICommonDialogSettings
    {
        /// <inheritdoc />
        public string? InitialDirectory { get; set; } = string.Empty;

        /// <inheritdoc />
        public string? Title { get; set; } = string.Empty;

        /// <summary>
        /// File dialog filter.
        /// </summary>
        public string Filter { get; set; } = string.Empty;

        /// <summary>
        /// Index of file dialog filters.
        /// </summary>
        public int FilterIndex { get; set; }

        /// <summary>
        /// File name.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// <see langword="true" /> if multiple files can be selected; otherwise <see langword="false" />.
        /// </summary>
        public bool CanMultiSelect { get; set; }

        /// <summary>
        /// File names list.
        /// </summary>
        public IList<string> FileNames { get; } = new List<string>();
    }
}
