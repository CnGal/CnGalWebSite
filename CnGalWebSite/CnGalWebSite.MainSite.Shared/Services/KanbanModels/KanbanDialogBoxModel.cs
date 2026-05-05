#nullable enable

namespace CnGalWebSite.MainSite.Shared.Services.KanbanModels
{
    public class KanbanDialogBoxModel
    {
        public required string Content { get; set; }
        public string? Expression { get; set; }
        public string? MotionGroup { get; set; }
        public int Motion { get; set; } = -1;
        public int Priority { get; set; }
        public KanbanDialogBoxType Type { get; set; }
    }

    public enum KanbanDialogBoxType
    {
        Text = 0,
        Image = 1,
        Notification = 2
    }
}
