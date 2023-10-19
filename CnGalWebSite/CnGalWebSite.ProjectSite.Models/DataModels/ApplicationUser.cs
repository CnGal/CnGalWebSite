using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace CnGalWebSite.ProjectSite.Models.DataModels
{
    public class ApplicationUser : IdentityUser
    {
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
        /// 注册时间
        /// </summary>
        public DateTime RegistTime { get; set; }

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
        /// 标签
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// 联系方式
        /// </summary>
        public string Contact { get; set; }

        /// <summary>
        /// 企划
        /// </summary>
        public List<Project> Projects { get; set; }

        /// <summary>
        /// 橱窗
        /// </summary>
        public List<Stall> Stalls { get; set; }

        /// <summary>
        /// 橱窗
        /// </summary>
        public List<UserImage> Images { get; set; }

        /// <summary>
        /// 橱窗
        /// </summary>
        public List<UserAudio> Audios { get; set; }

        /// <summary>
        /// 橱窗
        /// </summary>
        public List<UserText> Texts { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// 隐藏
        /// </summary>
        public bool Hide { get; set; }

        /// <summary>
        /// 既往作品
        /// </summary>
        public string PreviousWorks { get; set; }

        public string GetName(UserType? type = null)
        {
            if ((type ?? Type) == UserType.Organization)
            {
                return string.IsNullOrWhiteSpace(OrganizationName) ? UserName : OrganizationName;
            }
            else
            {
                return string.IsNullOrWhiteSpace(PersonName) ? UserName : PersonName;
            }
        }
    }

    public enum UserType
    {
        [Display(Name ="创作者")]
        Person,
        [Display(Name ="组织")]
        Organization
    }

    public class UserImage : BaseModel
    {
        public long Id { get; set; }

        public string Note { get; set; }

        public string Image { get; set; }

        /// <summary>
        /// 创建者
        /// </summary>
        public ApplicationUser CreateUser { get; set; }
        public string CreateUserId { get; set; }
    }

    public class UserAudio : BaseModel
    {
        public long Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 简介
        /// </summary>
        public string BriefIntroduction { get; set; }
        /// <summary>
        /// 链接
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 时长
        /// </summary>
        public TimeSpan Duration { get; set; }
        /// <summary>
        /// 缩略图
        /// </summary>
        public string Thumbnail { get; set; }

        /// <summary>
        /// 创建者
        /// </summary>
        public ApplicationUser CreateUser { get; set; }
        public string CreateUserId { get; set; }
    }

    public class UserText : BaseModel
    {
        public long Id { get; set; }

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

        /// <summary>
        /// 创建者
        /// </summary>
        public ApplicationUser CreateUser { get; set; }
        public string CreateUserId { get; set; }
    }

}
