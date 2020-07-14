using ERGLauncher.Core.DialogSettings.Implementations;
using ERGLauncher.Services;
using Microsoft.Extensions.Logging;

namespace ERGLauncher.Models.Implementations
{
    /// <summary>
    /// Message dialog model.
    /// </summary>
    public class MessageModel : ModelBase, IMessageModel
    {
        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<MessageModel> logger;

        /// <summary>
        /// Message.
        /// </summary>
        private string message;

        /// <summary>
        /// Detailed message.
        /// </summary>
        private string? details;

        /// <summary>
        /// <see langword="true" /> if show detailed message; otherwise <see langword="false" />
        /// </summary>
        private bool isShowDetails;

        /// <summary>
        /// Message dialog messageDialogSettings
        /// </summary>
        private MessageDialogSettings? messageDialogSettings;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="resourceService">Resource service</param>
        /// <param name="themeService">Theme service</param>
        public MessageModel(ILogger<MessageModel> logger, IResourceService resourceService, IThemeService themeService)
            : base(resourceService, themeService)
        {
            this.logger = logger;
            this.message = string.Empty;
            this.isShowDetails = false;
        }

        /// <inheritdoc />
        public string Message
        {
            get => this.message;
            set => this.SetProperty(ref this.message, value);
        }

        /// <inheritdoc />
        public string? Details
        {
            get => this.details;
            set => this.SetProperty(ref this.details, value);
        }

        /// <inheritdoc />
        public bool IsShowDetails
        {
            get => this.isShowDetails;
            set => this.SetProperty(ref this.isShowDetails, value);
        }

        /// <inheritdoc />
        public void LoadSettings(MessageDialogSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            this.messageDialogSettings = settings;
            this.Title = this.messageDialogSettings.Title;
            this.Height = this.messageDialogSettings.Height;
            this.Width = this.messageDialogSettings.Width;
            this.Message = this.messageDialogSettings.Message;
            this.Details = this.messageDialogSettings.Details;
            this.IsShowDetails = !string.IsNullOrEmpty(this.Details);
        }
    }
}
