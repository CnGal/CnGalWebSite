using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.OperationRecords
{
    public class OperationRecordOverviewModel
    {
        public long Id { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public OperationRecordType Type { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 用户Id
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 目标Id
        /// </summary>
        public string ObjectId { get; set; }
        /// <summary>
        /// Ip
        /// </summary>
        public string Ip { get; set; }
        /// <summary>
        /// Cookie
        /// </summary>
        public string Cookie { get; set; }
        /// <summary>
        /// 操作时间
        /// </summary>
        public DateTime OperationTime { get; set; }
    }
}
