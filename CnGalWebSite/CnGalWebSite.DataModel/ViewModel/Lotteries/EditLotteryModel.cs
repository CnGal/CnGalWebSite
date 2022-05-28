using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Lotteries
{
    public class EditLotteryModel : BaseEditModel
    {
        [Display(Name = "显示名称")]
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

        public override Result Validate()
        {
            //处理数据
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(DisplayName))
            {
                return new Result { Error= "请填写所有必填项目" };
            }

            if (BeginTime > EndTime)
            {
                return new Result { Error = "开始时间必须早于结束时间" };
            }

            if (Type == LotteryType.Automatic && LotteryTime < EndTime)
            {
                return new Result { Error = "结束时间必须早于抽奖时间" };
            }

            if (Awards.Count == 0)
            {
                return new Result { Error = "至少需要一个奖品" };
            }
            foreach (var item in Awards)
            {
                if (item.Type == LotteryAwardType.ActivationCode && item.Count != item.Prizes.Count)
                {
                    return new Result { Error = "激活码的数量需要对应实际数量" };
                }
            }
            return new Result { Successful = true };

        }
    }

    public class EditLotteryAwardModel : BaseEditModel
    {
        [Display(Name = "优先级")]
        public int Priority { get; set; }

        [Display(Name = "数量")]
        public int Count { get; set; }

        [Display(Name = "赞助商")]
        public string Sponsor { get; set; }

        [Display(Name = "图片")]
        public string Image { get; set; }

        [Display(Name = "链接")]
        public string Link { get; set; }

        [Display(Name = "类型")]
        public LotteryAwardType Type { get; set; }

        [Display(Name = "附加积分")]
        public int Integral { get; set; }

        public List<EditLotteryPrizeModel> Prizes { get; set; } = new List<EditLotteryPrizeModel>();

        public override Result Validate()
        {
            //处理数据
            if (string.IsNullOrWhiteSpace(Name))
            {
                return new Result { Error = "请填写所有必填项目" };
            }

            return new Result { Successful = true };
        }
    }

    public class EditLotteryPrizeModel
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "请填写激活码")]
        [Display(Name = "激活码")]
        public string Context { get; set; }
    }
}
