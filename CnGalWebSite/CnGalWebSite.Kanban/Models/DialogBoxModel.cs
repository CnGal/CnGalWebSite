using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.Models
{
    public class DialogBoxModel
    {
        public string Content { get; set; }

        public string Expression { get; set; }

        public string MotionGroup { get; set; }

        public int Motion { get; set; } = -1;

        public DialogBoxType Type { get; set; }
    }

    public enum DialogBoxType
    {
        Text,
        Success,
        Info,
        Warning,
        Error
    }
}
