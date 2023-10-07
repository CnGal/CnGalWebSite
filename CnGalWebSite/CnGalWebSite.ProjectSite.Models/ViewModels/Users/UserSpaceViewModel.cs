using CnGalWebSite.Core.Models;
using CnGalWebSite.ProjectSite.Models.ViewModels.Projects;
using CnGalWebSite.ProjectSite.Models.ViewModels.Stalls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Models.ViewModels.Users
{
    public class UserSpaceViewModel
    {
        /// <summary>
        /// tab栏索引缓存
        /// </summary>
        public int TabIndex { get; set; }

        /// <summary>
        /// 用户基础信息
        /// </summary>
        public UserInfoViewModel UserInfo { get; set; }=new UserInfoViewModel();

        /// <summary>
        /// 企划列表
        /// </summary>
        public List<ProjectInfoViewModel> Projects { get; set; }=new List<ProjectInfoViewModel>();

        /// <summary>
        /// 橱窗
        /// </summary>
        public List<StallInfoViewModel> Stalls { get; set; } = new List<StallInfoViewModel>();

        /// <summary>
        /// 图片
        /// </summary>
        public List<UserImageViewModel> Images { get; set; } = new List<UserImageViewModel>();

        /// <summary>
        /// 音频
        /// </summary>
        public List<EditAudioAloneModel> Audios { get; set; } = new List<EditAudioAloneModel>();

        /// <summary>
        /// 预览文本
        /// </summary>
        public List<UserTextViewModel> Texts { get; set; } = new List<UserTextViewModel>();
    }

    public class UserTextViewModel
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

    public class UserImageViewModel : BaseImageEditModel
    {

    }
}
