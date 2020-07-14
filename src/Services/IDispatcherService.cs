using System;
using System.Threading.Tasks;

namespace ERGLauncher.Services
{
    /// <summary>
    /// Dispatcher service interface.
    /// </summary>
    public interface IDispatcherService
    {
        /// <summary>
        /// Performs processing either synchronously or asynchronously.
        /// </summary>
        /// <param name="action">Processing</param>
        /// <returns>Task</returns>
        public ValueTask SafeAction(Action action);
    }
}
