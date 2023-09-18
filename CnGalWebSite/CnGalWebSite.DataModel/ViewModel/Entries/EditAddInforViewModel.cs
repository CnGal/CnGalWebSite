using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Entries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CnGalWebSite.DataModel.ViewModel
{
    public class EditAddInforViewModel : BaseEntryEditModel
    {
        /// <summary>
        /// 预约
        /// </summary>
        public EditBookingModel Booking { get; set; }=new EditBookingModel();
        /// <summary>
        /// STAFF
        /// </summary>
        public List<StaffModel> Staffs { get; set; } = new List<StaffModel>();
        /// <summary>
        /// 发行列表
        /// </summary>
        public List<EditReleaseModel> Releases { get; set; } = new List<EditReleaseModel>();
        /// <summary>
        /// 基础信息
        /// </summary>
        public List<EditInformationModel> Informations { get; set; } = new List<EditInformationModel>();
        [Display(Name = "制作组")]
        public string ProductionGroup { get; set; }
        [Display(Name = "发行商")]
        public string Publisher { get; set; }
        [Display(Name = "声优")]
        public string CV { get; set; }

        public override Result Validate()
        {
            //检查staff数据
            if (Staffs != null)
            {
                foreach (var item in Staffs)
                {
                    if (string.IsNullOrWhiteSpace(item.Name)&&string.IsNullOrWhiteSpace(item.Name))
                    {
                        return new Result { Error = $"请填写名称，并检查是否存在空行" };
                    }
                    if (string.IsNullOrWhiteSpace(item.PositionOfficial))
                    {
                        return new Result { Error = $"【{item.Name}】没有填写名称" };
                    }
                    if (string.IsNullOrWhiteSpace(item.Name))
                    {
                        return new Result { Error = $"【{item.PositionOfficial}】没有填写职位" };
                    }
                    if (Staffs.Where(s => s.Id != item.Id).Any(s => s.Name == item.Name && s.Modifier == item.Modifier && s.PositionOfficial == item.PositionOfficial))
                    {
                        return new Result { Error = $"重复的项目【{item.Modifier}{item.PositionOfficial}：{item.Name}】" };
                    }
                }
                
            }

            //检查预约数据
            if(Booking.Goals.Any(s=>string.IsNullOrWhiteSpace(s.Name)))
            {
                return new Result { Error = $"请填写预约目标名称" };
            }
            foreach(var item in Booking.Goals)
            {
                if(Booking.Goals.Count(s=>s.Name==item.Name)>1)
                {
                    return new Result { Error = $"重复的预约目标名称：{item.Name}" };
                }
            }

            return new Result { Successful = true };
        }

    }

    public class EditBookingModel
    {
        [Display(Name = "上线后通知预约用户")]
        public bool IsNeedNotification { get; set; }

        [Display(Name = "开启预约")]
        public bool Open { get; set; }

        [Display(Name = "关联抽奖")]
        public string LotteryName { get; set; }

        public List<EditBookingGoalModel> Goals { get; set; } = new List<EditBookingGoalModel>();
    }

    public class EditBookingGoalModel
    {
        [Display(Name = "名称")]
        public string Name { get; set; }
        [Display(Name = "人数")]
        public int Target { get; set; }
    }


    public class GamePlatformModel
    {
        public GamePlatformType GamePlatformType { get; set; }

        public bool IsSelected { get; set; }
    }

    public class StaffModel
    {
        public long Id { get; set; }
        [Display(Name = "分组")]
        public string Modifier { get; set; }

        [Display(Name = "★官方职位")]
        [Required(ErrorMessage = "请填写官方职位")]
        public string PositionOfficial { get; set; }

        [Display(Name = "★唯一名称")]
        [Required(ErrorMessage = "请填写唯一名称")]
        public string Name { get; set; }

        [Display(Name = "自定义名称")]
        public string CustomName { get; set; }

        [Display(Name = "通用职位")]
        public PositionGeneralType PositionGeneral { get; set; }

        [Display(Name = "隶属组织")]
        public string SubordinateOrganization { get; set; }

        public Result Validate(List<StaffModel> staffs)
        {
            if (staffs.Where(s => s.Id != Id).Any(s => s.Name == Name && s.Modifier == Modifier && s.PositionOfficial == PositionOfficial))
            {
                return new Result { Error = $"已存在【{Modifier}{PositionOfficial}：{Name}】" };
            }
            else
            {
                return new Result { Successful = true };
            }
        }
    }

    public class SocialPlatform
    {
        [Display(Name = "平台名称")]
        [Required(ErrorMessage = "请填写平台名称")]
        public string Name { get; set; }
        [Display(Name = "链接")]
        [Required(ErrorMessage = "请填写链接")]
        public string Link { get; set; }
    }

    public class EditReleaseModel
    {
        [Display(Name = "名称")]
        public string Name { get; set; }

        [Display(Name = "发行平台类型")]
        public PublishPlatformType PublishPlatformType { get; set; }

        [Display(Name = "发行平台名称")]
        public string PublishPlatformName { get; set; }

        [Display(Name = "类别")]
        public GameReleaseType Type { get; set; }

        [Display(Name = "游戏平台")]
        public List< GamePlatformType> GamePlatformTypes { get; set; } = new List<GamePlatformType>();


        [Display(Name = "发行时间")]
        public DateTime? Time { get; set; }

        [Display(Name = "发行时间备注")]
        public string TimeNote { get; set; }

        [Display(Name = "引擎")]
        public string Engine { get; set; }

        [Display(Name = "平台Id/链接")]
        public string Link { get; set; }
    }

    public class EditInformationModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public string Description { get; set; }

        public string Icon { get; set; }
    }
}
