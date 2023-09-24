using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Base;
using System;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Articles
{
    public class EditArticleMainViewModel : BaseEditModel
    {
        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// 简介
        /// </summary>
        public string BriefIntroduction { get; set; }

        /// <summary>
        /// 主图
        /// </summary>
        public string MainPicture { get; set; }

        /// <summary>
        /// 背景图
        /// </summary>
        public string BackgroundPicture { get; set; }

        /// <summary>
        /// 小背景图
        /// </summary>
        public string SmallBackgroundPicture { get; set; }

        /// <summary>
        /// 类别
        /// </summary>
        public ArticleType Type { get; set; }
        /// <summary>
        /// 动态类别
        /// </summary>
        public string NewsType { get; set; }

        /// <summary>
        /// 本人创作
        /// </summary>
        public bool IsCreatedByCurrentUser { get; set; }
        /// <summary>
        /// 原作者
        /// </summary>
        public string OriginalAuthor { get; set; }
        /// <summary>
        /// 原文链接
        /// </summary>
        public string OriginalLink { get; set; }
        /// <summary>
        /// 发布日期
        /// </summary>
        public DateTime PubishTime { get; set; }
        /// <summary>
        /// 动态发生时间
        /// </summary>
        public DateTime? RealNewsTime { get; set; }

        public override Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return new Result { Error = "请填写唯一名称" };
            }
            if (string.IsNullOrWhiteSpace(BriefIntroduction))
            {
                return new Result { Error = "请填写简介" };
            }
            if (string.IsNullOrWhiteSpace(DisplayName))
            {
                return new Result { Error = "请填写显示名称" };
            }

            return new Result { Successful = true };
        }


    }
}
