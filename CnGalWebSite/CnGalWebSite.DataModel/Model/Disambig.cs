using System.Collections.Generic;
namespace CnGalWebSite.DataModel.Model
{
    public class Disambig
    {
        public int Id { get; set; }

        /// <summary>
        /// 名称 为XX消歧义 是XXX的消歧义页面
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 简介
        /// </summary>
        public string BriefIntroduction { get; set; }
        /// <summary>
        /// 主图
        /// </summary>
        public string MainPicture { get; set; }
        /// <summary>
        /// 背景图
        /// </summary>
        public string BackgroundPicture { get; set; }
        /// <summary>
        /// 小背景图
        /// </summary>
        public string SmallBackgroundPicture { get; set; }
        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool IsHidden { get; set; }

        public ICollection<Article> Articles { get; set; }
        public ICollection<Entry> Entries { get; set; }

        /// <summary>
        /// 审核记录 也是编辑记录
        /// </summary>
        public ICollection<Examine> Examines { get; set; }

    }
}
