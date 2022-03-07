using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Lotteries
{
    public class EditLotteryModel
    {
        public long Id { get; set; }

        [Display(Name = "唯一名称")]
        [Required(ErrorMessage = "请填写唯一名称")]
        public string Name { get; set; }

        [Display(Name = "显示名称")]
        [Required(ErrorMessage = "请填写显示名称")]
        public string DisplayName { get; set; }

        [Display(Name = "简介")]
        public string BriefIntroduction { get; set; }

        [Display(Name = "主图")]
        public string MainPicture { get; set; }

        [Display(Name = "背景图")]
        public string BackgroundPicture { get; set; }

        [Display(Name = "小背景图")]
        public string SmallBackgroundPicture { get; set; }

        [Display(Name = "缩略图")]
        public string Thumbnail { get; set; }
        [Display(Name = "主页")]
        [StringLength(10000000)]
        public string MainPage { get; set; }

        [Display(Name = "类型")]
        public LotteryType Type { get; set; }
        [Display(Name = "参加抽奖的条件")]
        public LotteryConditionType ConditionType { get; set; }


        [Display(Name = "开始时间")]
        public DateTime BeginTime { get; set; }
        [Display(Name = "结束时间")]
        public DateTime EndTime { get; set; }
        [Display(Name = "抽奖时间")]
        public DateTime LotteryTime { get; set; }

        public List<EditLotteryAwardModel> Awards { get; set; } = new List<EditLotteryAwardModel>();

        [Display(Name = "备注")]
        public string Note { get; set; }
    }

    public class EditLotteryAwardModel
    {
        public long Id { get; set; }

        [Display(Name = "优先级")]
        public int Priority { get; set; }

        [Display(Name = "数量")]
        public int Count { get; set; }

        [Display(Name = "名称")]
        [Required(ErrorMessage = "请填写名称")]
        public string Name { get; set; }

        [Display(Name = "类型")]
        public LotteryAwardType Type { get; set; }

        [Display(Name = "附加积分")]
        public int Integral { get; set; }

        public List<EditLotteryPrizeModel> Prizes { get; set; } = new List<EditLotteryPrizeModel>();
    }

    public class EditLotteryPrizeModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "请填写奖品")]
        [Display(Name = "奖品")]
        public string Context { get; set; }
    }
}
