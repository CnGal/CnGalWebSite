using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Models.DataModels
{
    public class Stall : BaseModel
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
        public int Price {  get; set; }

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
        /// 预览图
        /// </summary>
        public List<StallImage> Images { get; set; }

        /// <summary>
        /// 预音频
        /// </summary>
        public List<StallAudio> Audios { get; set; }

        /// <summary>
        /// 预览文本
        /// </summary>
        public List<StallText> Texts { get; set; }

        /// <summary>
        /// 附加信息
        /// </summary>
        public List<StallInformation> Informations { get; set; }

        /// <summary>
        /// 创建者
        /// </summary>
        public ApplicationUser CreateUser { get; set; }
        public string CreateUserId { get; set; }
    }

    public class StallInformationType
    {
        public long Id { get; set; }

        /// <summary>
        /// 支持的类型
        /// </summary>
        public ProjectPositionType[] Types { get; set; } = Array.Empty<ProjectPositionType>();

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 介绍
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool Hide { get; set; }

        /// <summary>
        /// 在小卡片上隐藏
        /// </summary>
        public bool HideInfoCard { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }
    }

    public class StallInformation
    {
        public long Id { get; set; }

        public string Value { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public StallInformationType Type { get; set; }
        public long TypeId { get; set; }

    }

    public class StallImage
    {
        public long Id { get; set; }

        public string Note { get; set; }

        public string Image { get; set; }

        public int Priority { get; set; }
    }

    public class StallAudio
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
        /// 优先级
        /// </summary>
        public int Priority { get; set; }
        /// <summary>
        /// 时长
        /// </summary>
        public TimeSpan Duration { get; set; }
        /// <summary>
        /// 缩略图
        /// </summary>
        public string Thumbnail { get; set; }
    }

    public class StallText
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
    }
}
