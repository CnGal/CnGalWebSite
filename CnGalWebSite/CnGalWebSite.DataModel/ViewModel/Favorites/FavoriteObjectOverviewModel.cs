using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Favorites
{
    public class FavoriteObjectOverviewModel
    {
        public long Id { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public FavoriteObjectType Type { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string ObjectName { get; set; }
        /// <summary>
        /// 目标Id
        /// </summary>
        public long ObjectId { get; set; }
        /// <summary>
        /// 收藏时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
