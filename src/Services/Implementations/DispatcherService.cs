using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ERGLauncher.Services.Implementations
{
    /// <summary>
    /// Dispatcher service.
    /// </summary>
    public class DispatcherService : IDispatcherService
    {
        /// <summary>
        /// Dispatcher.
        /// </summary>
        private readonly Dispatcher dispatcher;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dispatcher">Dispatcher</param>
        public DispatcherService(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        /// <inheritdoc />
        public async ValueTask SafeAction(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (!this.dispatcher.CheckAccess())
            {
                await this.dispatcher.InvokeAsync(action);
            }
            else
            {
                action.Invoke();
            }
        }
    }
}
