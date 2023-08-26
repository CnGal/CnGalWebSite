
using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class CommentOverviewModel
    {
        public long Id { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public CommentType Type { get; set; }
        /// <summary>
        /// 评论时间
        /// </summary>
        public DateTime CommentTime { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 发表评论的用户Id
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 发表评论的用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 目标Id
        /// </summary>
        public string ObjectId { get; set; }
        /// <summary>
        /// 关联词条Id
        /// </summary>
        public string ObjectName { get; set; }

        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool IsHidden { get; set; }
    }
}
