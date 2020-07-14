namespace ERGLauncher.Core
{
    /// <summary>
    /// Product
    /// </summary>
    public class Product : Item
    {
        /// <summary>
        /// Executable file path.
        /// </summary>
        private string path = string.Empty;

        /// <summary>
        /// Brand name.
        /// </summary>
        private string brandName = string.Empty;

        /// <summary>
        /// Executable file path.
        /// </summary>
        public string Path
        {
            get => this.path;
            set => this.SetProperty(ref this.path, value);
        }

        /// <summary>
        /// Brand name.
        /// </summary>
        public string BrandName
        {
            get => this.brandName;
            set => this.SetProperty(ref this.brandName, value);
        }
    }
}
