using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Models.DataModels
{
    public class Message : BaseModel
    {
        public long Id { get; set; }

        public string Text { get; set; }

        public bool Read { get; set; }

        public MessageType Type { get; set; }

        public PageType PageType { get; set; }

        public long PageId { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }

    public enum MessageType
    {
        [Display(Name ="应征企划")]
        ApplyProjectPosition,
        [Display(Name ="选定应征创作者")]
        PassedPositionUser,
        [Display(Name = "回复留言")]
        Reply,
        [Display(Name = "购买橱窗")]
        ApplyStall,
        [Display(Name = "回复橱窗购买请求")]
        PassedStallUser,
    }
}
