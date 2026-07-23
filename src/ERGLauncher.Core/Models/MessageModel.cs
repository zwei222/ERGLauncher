using ERGLauncher.Core.Services;

namespace ERGLauncher.Core.Models;

public sealed class MessageModel : ModelBase, IMessageModel
{
    private string message = string.Empty;
    private string? details;
    private bool isShowDetails;

    public MessageModel(IResourceService? resourceService = null, IThemeService? themeService = null)
        : base(resourceService, themeService)
    {
    }

    public string Message { get => this.message; set => this.SetProperty(ref this.message, value); }
    public string? Details { get => this.details; set => this.SetProperty(ref this.details, value); }
    public bool IsShowDetails { get => this.isShowDetails; set => this.SetProperty(ref this.isShowDetails, value); }

    public void LoadSettings(MessageDialogSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        this.Title = settings.Title;
        this.Height = settings.Height;
        this.Width = settings.Width;
        this.Message = settings.Message;
        this.Details = settings.Details;
        this.IsShowDetails = !string.IsNullOrEmpty(settings.Details);
    }
}
