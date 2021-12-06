namespace CnGalWebSite.Shared.AppComponent.Normal.Cards
{
    public class MainImageCardModel
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
        /// <summary>
        /// 分类 底部显示文字
        /// </summary>
        public string Classification { get; set; }
    }
}
