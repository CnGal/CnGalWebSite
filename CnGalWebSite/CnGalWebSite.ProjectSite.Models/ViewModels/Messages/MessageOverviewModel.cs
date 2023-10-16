using CnGalWebSite.ProjectSite.Models.DataModels;

namespace CnGalWebSite.ProjectSite.Models.ViewModels.Messages
{
    public class MessageOverviewModel
    {
        public long Id { get; set; }

        public string Text { get; set; }

        public bool Read { get; set; }

        public PageType PageType { get; set; }

        public MessageType Type { get; set; }

        public long PageId { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public bool Hide { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
