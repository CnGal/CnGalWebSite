using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.Models
{
    public class ChatModel
    {
        public DateTime Time { get; set; }

        public string Image { get; set; }

        public string Text { get; set; }

        public bool IsUser {  get; set; }
    }
}
