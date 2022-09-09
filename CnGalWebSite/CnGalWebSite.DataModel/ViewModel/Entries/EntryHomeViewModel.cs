using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel
{
    public class EntryHomeViewModel
    {
        public List<MainImageCardModel> Games { get; set; } = new List<MainImageCardModel>();
        public List<MainImageCardModel> Groups { get; set; } = new List<MainImageCardModel>();
        public List<MainImageCardModel> Roles { get; set; } = new List<MainImageCardModel>();
        public List<MainImageCardModel> Staffs { get; set; } = new List<MainImageCardModel>();
    }
    public class MainImageCardModel
    {
        public long Id { get; set; }
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

        public bool IsOutlink { get; set; }
    }
}
