#nullable enable
using System.Collections.Generic;

namespace CnGalWebSite.MainSite.Shared.Services.KanbanModels
{
    public class KanbanEventModel
    {
        public int Priority { get; set; } = -1;
        public List<string> Contents { get; set; } = new();
        public string? Expression { get; set; }
        public string? MotionGroup { get; set; }
        public int Motion { get; set; } = -1;
        public KanbanDialogBoxType DialogType { get; set; }
    }
}
