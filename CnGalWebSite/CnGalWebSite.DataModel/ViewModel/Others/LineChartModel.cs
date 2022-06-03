using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Others
{
   public class LineChartModel
    {
        
        public EChartsOptionModel Options { get; set; } = new EChartsOptionModel();

        public LineChartType Type { get; set; }

        public DateTime AfterTime { get; set; } = DateTime.MinValue;
        public DateTime BeforeTime { get; set; } = DateTime.MaxValue;
    }

    public class LineChartSingleData
    {
        public double Count { get; set; }

        public DateTime Time { get; set; }
    }

    public enum LineChartType
    {
        [Display(Name ="用户")]
        User,
        [Display(Name = "词条")]
        Entry,
        [Display(Name = "文章")]
        Article,
        [Display(Name = "标签")]
        Tag,
        [Display(Name = "审核")]
        Examine,
        [Display(Name = "评论")]
        Comment,
        [Display(Name = "消息")]
        Message,
        [Display(Name = "文件")]
        File,
        [Display(Name = "互联网档案馆备份")]
        BackUpArchive,
        [Display(Name = "编辑")]
        Edit,
    }

}
