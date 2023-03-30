using CnGalWebSite.DrawingBed.Models.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DrawingBed.Models.ViewModels
{
    public class UploadResult
    {
        /// <summary>
        /// 是否被上传
        /// </summary>
        public bool Uploaded { get; set; }
        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 处理过的文件url
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 原始文件Url
        /// </summary>
        public string OriginalUrl { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { get; set; }
        /// <summary>
        /// 音频长度
        /// </summary>
        public TimeSpan? Duration { get; set; }
        /// <summary>
        /// 文件长度
        /// </summary>
        public long? FileSize { get; set; }
        /// <summary>
        /// 哈希值
        /// </summary>
        public string Sha1 { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public UploadFileType Type { get; set; }

    }

}
