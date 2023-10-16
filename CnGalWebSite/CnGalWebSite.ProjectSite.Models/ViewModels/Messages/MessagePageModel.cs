using CnGalWebSite.ProjectSite.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Models.ViewModels.Messages
{
    public class MessagePageModel
    {
        public MessageGroupModel Read { get; set; } = new MessageGroupModel();
        public MessageGroupModel UnRead { get; set; } = new MessageGroupModel();

        public int TabIndex { get; set; }
    }

    public class MessageGroupModel
    {
        public int MaxCount { get; set; } = 8;

        public int TotalPages => ((Items.Count - 1) / MaxCount) + 1;

        public int CurrentPage { get; set; } = 1;

        public List<MessageViewModel> Items { get; set; }=new List<MessageViewModel>();
    }

    public class MessageViewModel
    {
        public long Id { get; set; }

        public string Text { get; set; }

        public bool Read { get; set; }

        public PageType PageType { get; set; }

        public MessageType Type { get; set; }

        public long PageId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
