
using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class ListMessagesInforViewModel
    {
        public long ReadedCount { get; set; }

        public long NotReadedCount { get; set; }

        public long All { get; set; }
    }

    public class ListMessagesViewModel
    {
        public List<ListMessageAloneModel> Messages { get; set; }
    }
    public class ListMessageAloneModel
    {
        [Display(Name = "Id")]
        public long Id { get; set; }
        [Display(Name = "类型")]
        public MessageType? Type { get; set; } = null;
        [Display(Name = "发送时间")]
        public DateTime? PostTime { get; set; }
        [Display(Name = "标题")]
        public string Title { get; set; }
        [Display(Name = "内容")]
        public string Text { get; set; }
        [Display(Name = "目标用户Id")]
        public string ApplicationUserId { get; set; }
        [Display(Name = "显示头衔")]
        public string Rank { get; set; }
        [Display(Name = "链接")]
        public string Link { get; set; }

        [Display(Name = "附加信息")]
        public string AdditionalInfor { get; set; }
        [Display(Name = "链接标题")]
        public string LinkTitle { get; set; }
        [Display(Name = "头像")]
        public string Image { get; set; }

        [Display(Name = "是否已读")]
        public bool IsReaded { get; set; }
    }

    public class MessagesPagesInfor
    {
        public CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions Options { get; set; }
        public ListMessageAloneModel SearchModel { get; set; }

        public string Text { get; set; }
    }
}
