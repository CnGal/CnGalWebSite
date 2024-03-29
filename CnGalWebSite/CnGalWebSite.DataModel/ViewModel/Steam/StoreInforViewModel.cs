﻿using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Steam
{
    public class StoreInfoViewModel
    {
        /// <summary>
        /// 平台类型
        /// </summary>
        [Display(Name = "平台类型")]
        public PublishPlatformType PlatformType { get; set; }

        /// <summary>
        /// 平台名称
        /// </summary>
        [Display(Name = "平台名称")]
        public string PlatformName { get; set; }

        /// <summary>
        /// 链接
        /// </summary>
        [Display(Name = "链接")]
        public string Link { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [Display(Name = "名称")]
        public string Name { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [Display(Name = "状态")]
        public StoreState State { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        [Display(Name = "币种")]
        public CurrencyCode CurrencyCode { get; set; }

        /// <summary>
        /// 信息更新方式
        /// </summary>
        [Display(Name = "信息更新方式")]
        public StoreUpdateType UpdateType { get; set; }

        /// <summary>
        /// 原价
        /// </summary>
        [Display(Name = "原价")]
        public double? OriginalPrice { get; set; }

        /// <summary>
        /// 现价
        /// </summary>
        [Display(Name = "现价")]
        public double? PriceNow { get; set; }

        /// <summary>
        /// 折扣
        /// </summary>
        [Display(Name = "折扣")]
        public double? CutNow { get; set; }

        /// <summary>
        /// 史低
        /// </summary>
        [Display(Name = "史低")]
        public double? PriceLowest { get; set; }

        /// <summary>
        /// 最高折扣
        /// </summary>
        [Display(Name = "最高折扣")]
        public double? CutLowest { get; set; }

        /// <summary>
        /// 平均游玩时长 分钟
        /// </summary>
        [Display(Name = "平均游玩时长 分钟")]
        public int? PlayTime { get; set; }

        /// <summary>
        /// 评测数
        /// </summary>
        [Display(Name = "评测数")]
        public int? EvaluationCount { get; set; }
        /// <summary>
        /// 好评率
        /// </summary>
        [Display(Name = "好评率")]
        public double? RecommendationRate { get; set; }

        /// <summary>
        /// 估计拥有人数 上限
        /// </summary>
        [Display(Name = "估计拥有人数 上限")]
        public int? EstimationOwnersMax { get; set; }
        /// <summary>
        /// 估计拥有人数 下限
        /// </summary>
        [Display(Name = "估计拥有人数 下限")]
        public int? EstimationOwnersMin { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Display(Name = "更新时间")]
        public DateTime UpdateTime { get; set; }
    }
}
