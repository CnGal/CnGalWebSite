using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DrawingBed.Models.DataModels
{

    public class UploadRecord
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

    public enum UploadFileType
    {
        [Display(Name = "图片")]
        Image,
        [Display(Name = "音频")]
        Audio
    }
    public enum ImageAspectType
    {
        [Display(Name = "")]
        None,
        [Display(Name = "1_1")]
        _1_1,
        [Display(Name = "16_9")]
        _16_9,
        [Display(Name = "9_16")]
        _9_16,
        [Display(Name = "4_1A2")]
        _4_1A2
    }

}
