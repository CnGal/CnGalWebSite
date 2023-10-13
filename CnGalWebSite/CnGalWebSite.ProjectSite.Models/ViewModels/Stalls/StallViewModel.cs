using CnGalWebSite.Core.Models;
using CnGalWebSite.ProjectSite.Models.DataModels;
using CnGalWebSite.ProjectSite.Models.ViewModels.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Models.ViewModels.Stalls
{
    public class StallViewModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// 职位详情
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 报价
        /// </summary>
        public int Price { get; set; }

        /// <summary>
        /// 职位类型 大类
        /// </summary>
        public ProjectPositionType PositionType { get; set; }

        /// <summary>
        /// 职位类型名称
        /// </summary>
        public string PositionTypeName { get; set; }

        /// <summary>
        /// 职位类型 小类
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 联系方式
        /// </summary>
        public string Contact { get; set; }

        /// <summary>
        /// 截止日期
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 图片
        /// </summary>
        public List<StallImageViewModel> Images { get; set; }=new List<StallImageViewModel>();

        /// <summary>
        /// 创建者
        /// </summary>
        public UserInfoViewModel CreateUser { get; set; } = new UserInfoViewModel();

        /// <summary>
        /// 音频
        /// </summary>
        public List<EditAudioAloneModel> Audios { get; set; } = new List<EditAudioAloneModel>();

        /// <summary>
        /// 预览文本
        /// </summary>
        public List<StallTextViewModel> Texts { get; set; } = new List<StallTextViewModel>();

        /// <summary>
        /// 附加信息
        /// </summary>
        public List<StallInformationViewModel> Informations { get; set; }=new List<StallInformationViewModel>();

    }

    public class StallInformationViewModel
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public string Icon { get; set; }

        public int Priority { get; set; }
    }

    public class StallTextViewModel
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 链接
        /// </summary>
        public string Link { get; set; }
    }

    public class StallImageViewModel : BaseImageEditModel
    {

    }
}
