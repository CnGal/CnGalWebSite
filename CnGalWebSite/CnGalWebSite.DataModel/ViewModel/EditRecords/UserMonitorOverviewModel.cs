using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Linq;

namespace CnGalWebSite.DataModel.ViewModel.EditRecords
{
    public class UserMonitorOverviewModel
    {
        public long Id { get; set; }
        /// <summary>
        /// 词条Id
        /// </summary>
        public int EntryId { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public EntryType Type { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string EntryName { get; set; }
        /// <summary>
        /// 添加监视的时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
