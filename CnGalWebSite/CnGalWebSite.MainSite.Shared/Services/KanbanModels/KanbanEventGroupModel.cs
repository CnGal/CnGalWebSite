#nullable enable
using System.Collections.Generic;

namespace CnGalWebSite.MainSite.Shared.Services.KanbanModels
{
    public class KanbanEventGroupModel
    {
        public List<KanbanTimeEventModel> TimeEvents { get; set; } = new();
        public List<KanbanDateEventModel> DateEvents { get; set; } = new();
        public List<KanbanIdleEventModel> IdleEvents { get; set; } = new();
        public List<KanbanMouseOverEventModel> MouseOverEvents { get; set; } = new();
        public List<KanbanCustomEventModel> CustomEvents { get; set; } = new();
        public List<KanbanNavigationEventModel> NavigationEvents { get; set; } = new();
    }

    public class KanbanIdleEventModel : KanbanEventModel;

    public class KanbanTimeEventModel : KanbanEventModel
    {
        public TimeOnly AfterTime { get; set; }
        public TimeOnly BeforeTime { get; set; }

        public KanbanTimeEventModel()
        {
            Priority = 0;
        }
    }

    public class KanbanDateEventModel : KanbanEventModel
    {
        public DateOnly Date { get; set; }

        public KanbanDateEventModel()
        {
            Priority = 0;
        }
    }

    public class KanbanMouseOverEventModel : KanbanEventModel
    {
        public string? Selector { get; set; }
    }

    public class KanbanCustomEventModel : KanbanEventModel
    {
        public string? Name { get; set; }
    }

    public class KanbanNavigationEventModel : KanbanEventModel
    {
        public string? Url { get; set; }
    }
}
