using CnGalWebSite.DataModel.Model;
using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ExamineModel.Entries
{
    public class EntryAddInfor
    {
        public List<BasicEntryInformation_> Information { get; set; } = new List<BasicEntryInformation_> { };
        public List<EntryStaffExamineModel> Staffs { get; set; } = new List<EntryStaffExamineModel> { };
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
}
