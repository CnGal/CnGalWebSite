
using CnGalWebSite.DataModel.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class EntryOverviewModel
    {
        public int Id { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public EntryType Type { get; set; }
        /// <summary>
        /// 唯一名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }
        /// <summary>
        /// 隐藏
        /// </summary>
        public bool IsHidden { get; set; }
        /// <summary>
        /// 隐藏外链
        /// </summary>
        public bool IsHideOutlink { get; set; }
        /// <summary>
        /// 可否评论
        /// </summary>
        public bool CanComment { get; set; }
    }
}
