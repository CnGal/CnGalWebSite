#nullable enable
using System;

namespace CnGalWebSite.MainSite.Shared.Services.KanbanModels
{
    public class KanbanChatModel
    {
        public DateTime Time { get; set; }
        public string? Image { get; set; }
        public string? Text { get; set; }
        public bool IsUser { get; set; }
    }
}
