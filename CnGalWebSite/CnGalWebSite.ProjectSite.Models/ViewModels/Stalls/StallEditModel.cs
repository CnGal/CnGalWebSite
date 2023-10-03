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
        /// 联系方式
        /// </summary>
        public string Contact { get; set; }

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

        public override Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return new Result { Success = false, Message = "请填写橱窗名称" };
            }
            if (string.IsNullOrWhiteSpace(Contact))
            {
                return new Result { Success = false, Message = "请填写联系方式" };
            }
            if (string.IsNullOrWhiteSpace(Description) || Description.Length < 10)
            {
                return new Result { Success = false, Message = "请填写橱窗详情，并不少于10个字" };
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
                return new Result { Success = false, Message = "请填写剧本名称" };
            }
           
            if (string.IsNullOrWhiteSpace(Content) || Content.Length < 30)
            {
                return new Result { Success = false, Message = "请填写剧本内容，并不少于30个字" };
            }
            return new Result { Success = true };
        }
    }

    public class StallImageEditModel:BaseImageEditModel
    {
        public long Id { get; set; }
    }
}
