
using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class MessageOverviewModel
    {
        public long Id { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public MessageType Type { get; set; }
        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime PostTime { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// 目标用户Id
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 目标用户名称
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 链接
        /// </summary>
        public string Link { get; set; }
        /// <summary>
        /// 是否已读
        /// </summary>
        public bool IsReaded { get; set; }
    }
}
