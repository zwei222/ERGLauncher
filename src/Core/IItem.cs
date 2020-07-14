using System.Runtime.Serialization;
using System.Windows.Media.Imaging;
using Prism.Mvvm;

namespace ERGLauncher.Core
{
    /// <summary>
    /// Base item.
    /// </summary>
    public abstract class Item : BindableBase
    {
        /// <summary>
        /// Icon
        /// </summary>
        private BitmapImage? icon;

        /// <summary>
        /// Name
        /// </summary>
        private string name = string.Empty;

        /// <summary>
        /// Icon file path.
        /// </summary>
        private string? iconPath;

        /// <summary>
        /// Icon.
        /// </summary>
        [IgnoreDataMember]
        public BitmapImage? Icon
        {
            get => this.icon;
            set => this.SetProperty(ref this.icon, value);
        }

        /// <summary>
        /// Name.
        /// </summary>
        public string Name
        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }

        /// <summary>
        /// Icon file path.
        /// </summary>
        public string? IconPath
        {
            get => this.iconPath;
            set => this.SetProperty(ref this.iconPath, value);
        }
    }
}
