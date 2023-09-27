using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.Models
{
    public class EventGroupModel
    {
        public List<TimeEventModel> TimeEvents { get; set; } = new List<TimeEventModel>();

        public List<DateEventModel> DateEvents { get; set; } = new List<DateEventModel>();

        public List<IdleEventModel> IdleEvents { get; set; } = new List<IdleEventModel>();

        public List<MouseOverEventModel> MouseOverEvents { get; set; } = new List<MouseOverEventModel>();

        public List<CustomEventModel> CustomEvents { get; set; } = new List<CustomEventModel>();

        public List<NavigationEventModel> NavigationEvents { get; set; } = new List<NavigationEventModel>();
    }

    public class BaseEventModel
    {
        public int Id { get; set; }
        public int Priority { get; set; } = -1;
        public string Note { get; set; }
        public List<string> Contents { get; set; } = new List<string>();
        public DateTime LastTriggerTime { get; set; }
    }


    public class IdleEventModel : BaseEventModel
    {

    }

    public class TimeEventModel : BaseEventModel
    {
        public TimeOnly AfterTime { get; set; }
        public TimeOnly BeforeTime { get; set; }

        public TimeEventModel()
        {
            Priority = 0;
        }
    }

    public class DateEventModel : BaseEventModel
    {
        public DateOnly Date { get; set; }

        public DateEventModel()
        {
            Priority = 0;
        }
    }

    public class MouseOverEventModel : BaseEventModel
    {
        public string Selector { get; set; }
    }

    public class CustomEventModel : BaseEventModel
    {
        public string Name { get; set; }
    }

    public class NavigationEventModel : BaseEventModel
    {
        public string Url { get; set; }
    }
}
