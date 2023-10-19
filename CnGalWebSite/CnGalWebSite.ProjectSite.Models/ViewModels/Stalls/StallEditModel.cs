using CnGalWebSite.Core.Models;
using CnGalWebSite.ProjectSite.Models.Base;
using CnGalWebSite.ProjectSite.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Models.ViewModels.Stalls
{
    public class StallEditModel : BaseEditModel
    {
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
        /// 截止日期
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 图片
        /// </summary>
        public List<StallImageEditModel> Images { get; set; } = new List<StallImageEditModel>();

        /// <summary>
        /// 音频
        /// </summary>
        public List<EditAudioAloneModel> Audios { get; set; } = new List<EditAudioAloneModel>();

        /// <summary>
        /// 预览文本
        /// </summary>
        public List<StallTextEditModel> Texts { get; set; } = new List<StallTextEditModel>();

        /// <summary>
        /// 附加信息
        /// </summary>
        public List<StallInformationEditModel> Informations { get; set; } = new List<StallInformationEditModel>();


        public override Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return new Result { Success = false, Message = "请填写橱窗名称" };
            }
            if (EndTime <= DateTime.Now)
            {
                return new Result { Success = false, Message = "截止日期必须大于当前日期" };
            }


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

    public class StallTextEditModel:StallTextViewModel
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

    public class StallImageEditModel:BaseImageEditModel
    {
        public long Id { get; set; }
    }

    public class StallInformationEditModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public List<ProjectPositionType> Types { get; set; } =new List<ProjectPositionType>();

        public string Description { get; set; }

        public string Icon { get; set; }

        public long TypeId { get; set; }

    }
}
