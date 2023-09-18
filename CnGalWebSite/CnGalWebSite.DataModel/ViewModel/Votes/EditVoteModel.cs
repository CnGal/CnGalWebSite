using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace CnGalWebSite.DataModel.ViewModel.Votes
{
    public class EditVoteModel:BaseEditModel
    {
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
        public VoteType Type { get; set; }

        [Display(Name = "开始时间")]
        public DateTime BeginTime { get; set; }

        [Display(Name = "截止时间")]
        public DateTime EndTime { get; set; }

        [Display(Name = "允许用户重新投票")]
        public bool IsAllowModification { get; set; }
        [Display(Name = "同时选中项下限")]
        public long MinimumSelectionCount { get; set; }
        [Display(Name = "同时选中项上限")]
        public long MaximumSelectionCount { get; set; }

        public List<RelevancesModel> Roles { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> Staffs { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> Groups { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> Games { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> Peripheries { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> Articles { get; set; } = new List<RelevancesModel>();
        public List<EditVoteOptionModel> Options { get; set; } = new List<EditVoteOptionModel>();

        public override Result Validate()
        {
            //处理数据
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(DisplayName))
            {
                return new Result { Successful = false, Error = "请填写所有必填项目" };
            }

            if (BeginTime > EndTime)
            {
                return new Result { Successful = false, Error = "开始时间必须早于结束时间" };

            }

            if (Type == VoteType.MultipleChoice)
            {
                if (MinimumSelectionCount < 1)
                {
                    return new Result { Successful = false, Error = "只能最少同时选中项目数必须大于0" };
                }
                if (MaximumSelectionCount > Options.Count)
                {
                    return new Result { Successful = false, Error = "只能最多同时选中项目数必须小于等于选项数" };
                }
                if (MinimumSelectionCount > MaximumSelectionCount)
                {
                    return new Result { Successful = false, Error = "只能最少同时选中项目数必须小于等于最多同时选中项目数" };
                }
            }

            return new Result { Successful = true };
        }

      
    }
    public class EditVoteOptionModel
        {
            [Display(Name = "临时Id")]
            public long TempId { get; set; }

            [Display(Name = "真实Id")]
            public long RealId { get; set; }

            [Display(Name = "内容")]
            public string Text { get; set; }

            [Display(Name = "类型")]
            public VoteOptionType Type { get; set; }

            public Result Validate(List<EditVoteOptionModel> options)
            {
                if (string.IsNullOrWhiteSpace(Text))
                {
                    return new Result { Error = $"选项不能为空" };
                }

                if (options.Where(s => s.RealId != RealId).Any(s => s.Text == Text && s.Type == Type))
                {
                    return new Result { Error = $"已存在【{Type}：{Text}】" };
                }
                else
                {
                    return new Result { Successful = true };
                }
            }
        }
}
