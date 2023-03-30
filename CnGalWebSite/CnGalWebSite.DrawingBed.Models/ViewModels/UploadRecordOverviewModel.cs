using CnGalWebSite.DrawingBed.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DrawingBed.Models.ViewModels
{
    public class UploadRecordOverviewModel
    {
        public long Id { get; set; }

        public string Sha1 { get; set; }

        /// <summary>
        /// 文件链接
        /// </summary>
        public string Url { get; set; }

        public long? Size { get; set; }

        public DateTime UploadTime { get; set; }

        /// <summary>
        /// 音频长度
        /// </summary>
        public TimeSpan? Duration { get; set; }
        /// <summary>
        /// 文件类型
        /// </summary>
        public UploadFileType Type { get; set; }

        public string UserId { get; set; }
    }
}
