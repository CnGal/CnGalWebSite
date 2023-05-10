using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Home
{
    public class HomeItemModel
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 主图
        /// </summary>
        public string Image { get; set; }
        /// <summary>
        /// 跳转链接
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 阅读数
        /// </summary>
        public int ReadCount { get; set; }
        /// <summary>
        /// 评论数
        /// </summary>
        public int CommentCount { get; set; }

    }

    public class AnnouncementItemModel : HomeItemModel
    {
        public int Priority { get; set; }
    }
    public class FriendLinkItemModel : HomeItemModel
    {

    }
    public class LatastArticleItemModel : HomeItemModel
    {
        public string UserImage { get; set; }
        public string UserName { get; set; }

        public string PublishTime { get; set; }

        public string BriefIntroduction { get; set; }
        public string OriginalAuthor { get; set; }
    }

    public class LatastVideoItemModel : HomeItemModel
    {
        public string OriginalAuthor { get; set; }

        public string PublishTime { get; set; }

    }
    public class PublishedGameItemModel : HomeItemModel
    {
        public string BriefIntroduction { get; set; }
    }

    public class RecentlyEditedGameItemModel : HomeItemModel
    {
        public string PublishTime { get; set; }
    }

    public class UpcomingGameItemModel : HomeItemModel
    {
        public string BriefIntroduction { get; set; }
        public string PublishTime { get; set; }
    }
}
