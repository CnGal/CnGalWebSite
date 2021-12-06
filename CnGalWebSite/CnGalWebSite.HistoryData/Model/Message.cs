using System;
using System.ComponentModel.DataAnnotations;
#if NET5_0_OR_GREATER
#else
using CnGalWebSite.HistoryData.Model;
#endif
namespace CnGalWebSite.DataModel.Model
{
    public class Message
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public DateTime PostTime { get; set; }

        public string Rank { get; set; }

        public string Text { get; set; }

        public string Link { get; set; }

        public string AdditionalInfor { get; set; }

        public string LinkTitle { get; set; }

        public string Image { get; set; }

        public bool IsReaded { get; set; }

        public MessageType Type { get; set; }

        public string ApplicationUserId { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
    }

    public enum MessageType
    {
        [Display(Name = "审核通过")]
        ExaminePassed,
        [Display(Name = "审核驳回")]
        ExamineUnPassed,
        [Display(Name = "文章被回复")]
        ArticleReply,
        [Display(Name = "评论被回复")]
        CommentReply,
        [Display(Name = "空间被留言")]
        SpaceReply
    }
}
