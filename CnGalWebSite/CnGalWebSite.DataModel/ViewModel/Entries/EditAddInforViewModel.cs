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

        [Display(Name = "相关网站")]
        public List<SocialPlatform> SocialPlatforms { get; set; } = new List<SocialPlatform>() { };

        /// <summary>
        /// 预约
        /// </summary>
        public EditBookingModel Booking { get; set; }=new EditBookingModel();

        #region 游戏
        [Display(Name = "原作")]
        public string Original { get; set; }
        [Display(Name = "制作组")]
        public string ProductionGroup { get; set; }
        [Display(Name = "发行商")]
        public string Publisher { get; set; }
        [Display(Name = "官网")]
        public string OfficialWebsite { get; set; }
        [Display(Name = "Steam平台Id")]
        public string SteamId { get; set; }
        [Display(Name = "QQ群")]
        public string QQgroupGame { get; set; }
        [Display(Name = "STAFF")]
        public List<StaffModel> Staffs { get; set; } = new List<StaffModel>();

        public List<EditReleaseModel> Releases { get; set; } = new List<EditReleaseModel>();
        #endregion
        #region 角色

        [Display(Name = "声优")]
        public string CV { get; set; }
        [Display(Name = "性别")]
        public GenderType Gender { get; set; }
        [Display(Name = "身材数据")]
        public string FigureData { get; set; }
        [Display(Name = "身材(主观)")]
        public string FigureSubjective { get; set; }
        [Display(Name = "生日")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? Birthday { get; set; }
        [Display(Name = "发色")]
        public string Haircolor { get; set; }
        [Display(Name = "瞳色")]
        public string Pupilcolor { get; set; }
        [Display(Name = "服饰")]
        public string ClothesAccessories { get; set; }
        [Display(Name = "性格")]
        public string Character { get; set; }
        [Display(Name = "角色身份")]
        public string RoleIdentity { get; set; }
        [Display(Name = "血型")]
        public string BloodType { get; set; }
        [Display(Name = "身高")]
        public string RoleHeight { get; set; }
        [Display(Name = "兴趣")]
        public string RoleTaste { get; set; }
        [Display(Name = "年龄")]
        public string RoleAge { get; set; }

        #endregion
        #region Staff
        [Display(Name = "微博平台Id")]
        public string WeiboId { get; set; }
        [Display(Name = "Bilibili平台Id")]
        public string BilibiliId { get; set; }
        [Display(Name = "AcFun平台Id")]
        public string AcFunId { get; set; }
        [Display(Name = "知乎平台Id")]
        public string ZhihuId { get; set; }
        [Display(Name = "爱发电平台Id（不包括@）")]
        public string AfdianId { get; set; }
        [Display(Name = "Pixiv平台Id")]
        public string PixivId { get; set; }
        [Display(Name = "Twitter平台Id")]
        public string TwitterId { get; set; }
        [Display(Name = "YouTube平台Id")]
        public string YouTubeId { get; set; }
        [Display(Name = "Facebook平台Id")]
        public string FacebookId { get; set; }

        [Display(Name = "姓名")]
        public string RealName { get; set; }

        #endregion

        #region 制作组
        [Display(Name = "QQ群")]
        public string QQgroupGroup { get; set; }
        #endregion

        public override Result Validate()
        {
            //调整时间
            if (Birthday != null)
            {
                Birthday = Birthday.Value.AddHours(Birthday.Value.Hour < 12 ? (12 - Birthday.Value.Hour) : 0);
            }

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
}
