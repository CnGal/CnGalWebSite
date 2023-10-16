using CnGalWebSite.ProjectSite.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Models.ViewModels.Messages
{
    public class PutMessageModel
    {
        public string Text { get; set; }

        public bool Read { get; set; }

        public PageType PageType { get; set; }

        public MessageType Type { get; set; }

        public long PageId { get; set; }

        public string UserId { get; set; }
    }
}
