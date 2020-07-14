using System;
using System.Windows.Controls;
using MahApps.Metro.Controls;

namespace ERGLauncher.Views
{
    /// <summary>
    /// Base view.
    /// </summary>
    public abstract class ViewBase : MetroWindow
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        protected ViewBase()
        {
            this.TitleCharacterCasing = CharacterCasing.Normal;
        }

        /// <inheritdoc />
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (this.DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
