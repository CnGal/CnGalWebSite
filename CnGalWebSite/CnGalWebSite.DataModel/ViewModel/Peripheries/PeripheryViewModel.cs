using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Search;
using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Peripheries
{
    public class PeripheryViewModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string BriefIntroduction { get; set; }

        public bool IsHidden { get; set; }

        public bool IsEdit { get; set; }

        public bool IsCollected { get; set; }

        public EditState ImagesState { get; set; }

        public EditState MainState { get; set; }

        public EditState RelatedEntriesState { get; set; }

        public EditState RelatedPeripheriesState { get; set; }

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
        /// 是否为装饰品
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
        /// <summary>
        /// 阅读数
        /// </summary>
        public int ReaderCount { get; set; }
        /// <summary>
        /// 收集数
        /// </summary>
        public int CollectedCount { get; set; }
        /// <summary>
        /// 是否可以评论
        /// </summary>
        public bool CanComment { get; set; } = true;
        /// <summary>
        /// 评论数
        /// </summary>
        public int CommentCount { get; set; }
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
        /// 缩略图
        /// </summary>
        public string Thumbnail { get; set; }
        /// <summary>
        /// 关联词条
        /// </summary>
        public List<EntryInforTipViewModel> Entries { get; set; } = new List<EntryInforTipViewModel>();
        /// <summary>
        /// 关联周边
        /// </summary>
        public List<PeripheryInforTipViewModel> Peripheries { get; set; } = new List<PeripheryInforTipViewModel>();

        /// <summary>
        /// 关联周边合集
        /// </summary>
        public List<GameOverviewPeripheriesModel> PeripheryOverviewModels { get; set; } = new List<GameOverviewPeripheriesModel>();

        /// <summary>
        /// 图片列表
        /// </summary>
        public List<PicturesViewModel> Pictures { get; set; } = new List<PicturesViewModel>();
    }
}
