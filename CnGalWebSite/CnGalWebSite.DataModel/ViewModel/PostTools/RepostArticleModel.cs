using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.PostTools
{
    public class RepostArticleModel : ToolTaskBase
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string OriginalAuthor { get; set; }

        public string Url { get; set; }

        public string MainPage { get; set; }

        public string Image { get; set; }

        public bool IsCutImage { get; set; }

        public DateTime PublishTime { get; set; }


        public ArticleType Type { get; set; }

        public string UserName { get; set; }

        public bool IsCreatedByCurrentUser { get; set; }

        public RepostArticleModel()
        {
            TotalTaskCount = 4;
            Type = ArticleType.Evaluation;
        }
    }

    public enum RepostArticleType
    {
        ZhiHu,
        XiaoHeiHe,
        Bilibili
    }
    public enum OutputLevel
    {
        Infor,
        Warning,
        Dager
    }

}
