using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.Model
{
    public class Periphery
    {

        public long Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; }

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
        /// 缩略图
        /// </summary>
        public string Thumbnail { get; set; }

        /// <summary>
        /// 分类
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
        public bool IsReprint { get; set; }
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
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; } = 0;

        /// <summary>
        /// 最后编辑时间
        /// </summary>
        public DateTime LastEditTime { get; set; }

        /// <summary>
        /// 阅读数
        /// </summary>
        public int ReaderCount { get; set; }

        /// <summary>
        /// 收集到该周边的用户数
        /// </summary>
        public int CollectedCount { get; set; }

        /// <summary>
        /// 评论数
        /// </summary>
        public int CommentCount { get; set; }

        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool IsHidden { get; set; } = false;

        /// <summary>
        /// 是否可以评论
        /// </summary>
        public bool? CanComment { get; set; } = true;

        /// <summary>
        /// 审核记录 也是编辑记录
        /// </summary>
        public ICollection<Examine> Examines { get; set; } =new List<Examine>();
        /// <summary>
        /// 拥有该周边的用户
        /// </summary>
        public ICollection<PeripheryRelevanceUser> Users { get; set; }=new List<PeripheryRelevanceUser>();
        /// <summary>
        /// 关联词条
        /// </summary>
        [Obsolete("关联词条已迁移，请访问RelatedEntries")]
        public ICollection<PeripheryRelevanceEntry> Entries { get; set; }=new List<PeripheryRelevanceEntry>();
        /// <summary>
        /// 关联词条
        /// </summary>
        public virtual ICollection<Entry> RelatedEntries { get; set; }= new List<Entry>();
        /// <summary>
        /// 图片列表
        /// </summary>
        public ICollection<EntryPicture> Pictures { get; set; }=new List<EntryPicture>();
        /// <summary>
        /// 关联周边列表
        /// </summary>
        public virtual ICollection<PeripheryRelation> PeripheryRelationFromPeripheryNavigation { get; set; } = new List<PeripheryRelation>();

        /// <summary>
        /// To 指当前周边被关联的其他周边关联 呈现编辑视图时不使用
        /// </summary>
        public virtual ICollection<PeripheryRelation> PeripheryRelationToPeripheryNavigation { get; set; } = new List<PeripheryRelation>();

        public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
    }

    public class PeripheryRelevanceUser
    {
        public long Id { get; set; }

        public long PeripheryId { get; set; }
        public Periphery Periphery { get; set; }

        public DateTime StartOwnedTime { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }

    public class PeripheryRelevanceEntry
    {
        public long Id { get; set; }

        public long? PeripheryId { get; set; }
        public Periphery Periphery { get; set; }

        public int? EntryId { get; set; }
        public Entry Entry { get; set; }
    }
    public partial class PeripheryRelation
    {
        public long PeripheryRelationId { get; set; }

        public long? FromPeriphery { get; set; }
        public long? ToPeriphery { get; set; }

        public virtual Periphery FromPeripheryNavigation { get; set; } = new Periphery();
        public virtual Periphery ToPeripheryNavigation { get; set; } = new Periphery();
    }
    public enum PeripheryType
    {
        [Display(Name = "其他周边类型")]
        None,
        [Display(Name = "设定集或画册等")]
        SetorAlbumEtc,
        [Display(Name = "原声集")]
        Ost,
        [Display(Name = "套装")]
        Set,
    }
}
