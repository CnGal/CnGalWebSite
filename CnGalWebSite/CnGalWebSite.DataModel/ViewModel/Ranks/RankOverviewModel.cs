using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Ranks
{

    public class RankBaseModel
    {
        public long Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 显示文本
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public RankType Type { get; set; }
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }
        /// <summary>
        /// 人数
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// CSS
        /// </summary>
        public string CSS { get; set; }
        /// <summary>
        /// Styles
        /// </summary>
        public string Styles { get; set; }
        /// <summary>
        /// 图片
        /// </summary>
        public string Image { get; set; }
    }

    public class RankOverviewModel : RankBaseModel
    {

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 最后编辑时间
        /// </summary>
        public DateTime LastEditTime { get; set; }
        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool IsHidden { get; set; } = false;
    }

    public class RankEditModel : RankBaseModel
    {

    }
}
