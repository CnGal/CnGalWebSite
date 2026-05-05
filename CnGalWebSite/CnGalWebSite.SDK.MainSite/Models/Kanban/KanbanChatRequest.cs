namespace CnGalWebSite.SDK.MainSite.Models.Kanban;

public sealed class KanbanChatRequest
{
    public string Message { get; set; } = string.Empty;

    public bool IsFirst { get; set; }

    public string DeviceIdentification { get; set; } = string.Empty;
}
