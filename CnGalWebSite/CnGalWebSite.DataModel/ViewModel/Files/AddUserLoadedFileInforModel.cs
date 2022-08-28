using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Files
{
    public class AddUserUploadFileInforModel
    {
        public string FileName { get; set; }

        public long? FileSize { get; set; }

        /// <summary>
        /// 音频长度
        /// </summary>
        public TimeSpan? Duration { get; set; }

        public string Sha1 { get; set; }

        public UploadFileType Type { get; set; }
    }

    public enum UploadFileType
    {
        [Display(Name ="图片")]
        Image,
        [Display(Name = "音频")]
        Audio
    }
}
