using CnGalWebSite.DataModel.Model;
namespace CnGalWebSite.DataModel.ExamineModel.Peripheries
{
    public class PeripheryMain_1_0
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public PeripheryType Type { get; set; }
        #region 通用属性
        /// <summary>
        /// 尺寸
        /// </summary>
        public string Size { get; set; }
        /// <summary>
        /// 类别
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// 品牌
        /// </summary>
        public string Brand { get; set; }
        /// <summary>
        /// 作者
        /// </summary>
        public string Author { get; set; }
        /// <summary>
        /// 材质
        /// </summary>
        public string Material { get; set; }
        /// <summary>
        /// 单独部件数量
        /// </summary>
        public string IndividualParts { get; set; }
        /// <summary>
        /// 贩售链接
        /// </summary>
        public string SaleLink { get; set; }
        /// <summary>
        /// 价格
        /// </summary>
        public string Price { get; set; }
        /// <summary>
        /// 是否再版
        /// </summary>
        public bool IsReprint { get; set; } = true;
        /// <summary>
        /// 是否为可用物品
        /// </summary>
        public bool IsAvailableItem { get; set; }
        #endregion
        #region 设定集
        /// <summary>
        /// 页数
        /// </summary>
        public int PageCount { get; set; }
        #endregion
        #region 设定集
        /// <summary>
        /// 歌曲数
        /// </summary>
        public int SongCount { get; set; }
        #endregion

        public string BriefIntroduction { get; set; }

        public string MainPicture { get; set; }

        public string Thumbnail { get; set; }

        public string BackgroundPicture { get; set; }

        public string SmallBackgroundPicture { get; set; }

    }
}
