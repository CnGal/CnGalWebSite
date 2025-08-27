using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.EventBus.Models
{
    public class KanbanGroupGptModel
    {
        public List<KanbanGroupMessageModel> Messages { get; set; } = [];
    }

    public class KanbanGroupMessageModel
    {
        public string Name { get; set; }

        public long Id { get; set; }

        public string Text { get; set; }

        public bool IsAssistant { get; set; }
    }
}
