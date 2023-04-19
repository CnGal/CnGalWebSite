using CnGalWebSite.DataModel.ExamineModel.Shared;
using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ExamineModel.Entries
{
    public class EntryAddInfor
    {
        public List<BasicEntryInformation_> Information { get; set; } = new List<BasicEntryInformation_> { };
        public List<EntryStaffExamineModel> Staffs { get; set; } = new List<EntryStaffExamineModel> { };
        public List<GameReleaseExamineModel> Releases { get; set; } = new List<GameReleaseExamineModel> { };

        public EditBooking Booking { get; set; }=new EditBooking();
    }

    public class EditBooking
    {
        public List<ExamineMainAlone> MainInfor { get; set; } = new List<ExamineMainAlone>();

        public List<EditBookingGoal> Goals { get; set; } = new List<EditBookingGoal> { };
    }

    public class EditBookingGoal
    {
        public bool IsDelete { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// 当前目标达成的最低人数
        /// </summary>
        public int Target { get; set; }
    }

    public class BasicEntryInformation_
    {
        public bool IsDelete { get; set; }
        /// <summary>
        /// 主索引
        /// </summary>
        public string DisplayName { get; set; }

        public string DisplayValue { get; set; }
        public string Modifier { get; set; }
        public List<BasicEntryInformationAdditional_> Additional { get; set; } = new List<BasicEntryInformationAdditional_>();
    }

    public class BasicEntryInformationAdditional_
    {
        public bool IsDelete { get; set; }
        /// <summary>
        /// 主索引
        /// </summary>
        public string DisplayName { get; set; }
        public string DisplayValue { get; set; }

    }

    public class EntryStaffExamineModel
    {
        public bool IsDelete { get; set; }
        /// <summary>
        /// 关联的StaffId 不存在则为空
        /// </summary>
        public int? StaffId { get; set; }
        /// <summary>
        /// 仅当关联Staff为空时展示
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 优先展示自定义名称 与其他地方不一致的行为
        /// </summary>
        public string CustomName { get; set; }
        /// <summary>
        /// 官方职位
        /// </summary>
        public string PositionOfficial { get; set; }
        /// <summary>
        /// 通用职位
        /// </summary>
        public PositionGeneralType PositionGeneral { get; set; }
        /// <summary>
        /// 隶属组织
        /// </summary>
        public string SubordinateOrganization { get; set; }
        /// <summary>
        /// 分组
        /// </summary>
        public string Modifier { get; set; }
    }

    public class GameReleaseExamineModel
    {
        public bool IsDelete { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 发行平台类型
        /// </summary>
        public PublishPlatformType PublishPlatformType { get; set; }

        /// <summary>
        /// 发行平台名称
        /// </summary>
        public string PublishPlatformName { get; set; }

        /// <summary>
        /// 类别
        /// </summary>
        public GameReleaseType Type { get; set; }

        /// <summary>
        /// 游戏平台
        /// </summary>
        public GamePlatformType[] GamePlatformTypes { get; set; } = new GamePlatformType[0];


        /// <summary>
        /// 发行时间
        /// </summary>
        public DateTime? Time { get; set; }

        /// <summary>
        /// 发行时间备注
        /// </summary>
        public string TimeNote { get; set; }

        /// <summary>
        /// 引擎
        /// </summary>
        public string Engine { get; set; }

        /// <summary>
        /// 链接
        /// </summary>
        public string Link { get; set; }
    }
}
