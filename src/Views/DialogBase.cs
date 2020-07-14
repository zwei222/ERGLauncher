using System.Windows;
using Prism.Services.Dialogs;

namespace ERGLauncher.Views
{
    /// <summary>
    /// Base dialog View.
    /// </summary>
    public partial class DialogBase : ViewBase, IDialogWindow
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public DialogBase()
            : base()
        {
            if (this.Owner == null && Application.Current.MainWindow != null)
            {
                this.Owner = Application.Current.MainWindow;
            }

            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            void LoadedHandler(object sender, RoutedEventArgs e)
            {
                if (this.DataContext is IDialogAware dialogAware && !string.IsNullOrEmpty(dialogAware.Title))
                {
                    this.Title = dialogAware.Title;
                }

                this.Loaded -= LoadedHandler;
            }

            this.Loaded += LoadedHandler;
        }

        /// <summary>
        /// Dialog result
        /// </summary>
        public IDialogResult? Result { get; set; }
    }
}
