using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Anniversaries
{
    public enum JudgableGamesSortType
    {
        [Display(Name ="随机")]
        Random,
        [Display(Name = "发行时间")]
        PublishTime,
        [Display(Name = "最近评价")]
        LastScoreTime,
        [Display(Name = "评价数")]
        ScoreCount,
        [Display(Name = "最近编辑")]
        LastEditTime,
        [Display(Name = "热度")]
        ReadCount,

    }

    public enum JudgableGamesDisplayType
    {
        [Display(Name = "大卡片")]
        LargeCard,
        [Display(Name = "预览图")]
        SmallCard,
        [Display(Name = "条幅")]
        LongCard,
    }
}
