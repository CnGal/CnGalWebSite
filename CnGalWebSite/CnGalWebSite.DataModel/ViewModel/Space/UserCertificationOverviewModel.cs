using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.OperationRecords;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Space
{
    public class UserCertificationOverviewModel
    {
        public long Id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 用户Id
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 词条名称
        /// </summary>
        public string EntryName { get; set; }
        /// <summary>
        /// 词条Id
        /// </summary>
        public int EntryId { get; set; }
        /// <summary>
        /// 词条类型
        /// </summary>
        public EntryType EntryType { get; set; }
        /// <summary>
        /// 认证时间
        /// </summary>
        public DateTime CertificationTime { get; set; }
    }
}
