using CnGalWebSite.Core.Models;
using CnGalWebSite.ProjectSite.Models.Base;
using CnGalWebSite.ProjectSite.Models.DataModels;
using CnGalWebSite.ProjectSite.Models.ViewModels.Stalls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Models.ViewModels.Users
{
    public class UserEditModel: BaseEditModel
    {
        public new string Id { get; set; }

        /// <summary>
        /// 个人介绍
        /// </summary>
        public string PersonDescription { get; set; }

        /// <summary>
        /// 组织介绍
        /// </summary>
        public string OrganizationDescription { get; set; }

        /// <summary>
        /// 个人网名
        /// </summary>
        public string PersonName { get; set; }

        /// <summary>
        /// 组织名称
        /// </summary>
        public string OrganizationName { get; set; }

        /// <summary>
        /// 用户头像
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// 用户空间头图
        /// </summary>
        public string BackgroundImage { get; set; }

        /// <summary>
        /// 身份
        /// </summary>
        public UserType Type { get; set; }

        /// <summary>
        /// 图片
        /// </summary>
        public List<UserImageEditModel> Images { get; set; } = new List<UserImageEditModel>();

        /// <summary>
        /// 音频
        /// </summary>
        public List<EditAudioAloneModel> Audios { get; set; } = new List<EditAudioAloneModel>();

        /// <summary>
        /// 预览文本
        /// </summary>
        public List<UserTextEditModel> Texts { get; set; } = new List<UserTextEditModel>();


        public override Result Validate()
        {
            foreach (var item in Texts)
            {
                var result = item.Validate();
                if (!result.Success)
                {
                    return result;
                }
            }

            return new Result { Success = true };
        }
    }

    public class UserTextEditModel : UserTextViewModel
    {
        public long Id { get; set; }

        public Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return new Result { Success = false, Message = "请填写作品名称" };
            }

            if (string.IsNullOrWhiteSpace(Content))
            {
                return new Result { Success = false, Message = "请填写作品描述" };
            }

            if (string.IsNullOrWhiteSpace(Link))
            {
                return new Result { Success = false, Message = "请填写作品链接" };
            }
            return new Result { Success = true };
        }
    }

    public class UserImageEditModel : BaseImageEditModel
    {
        public long Id { get; set; }
    }
}
