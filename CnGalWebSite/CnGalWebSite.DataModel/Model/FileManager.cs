using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Files;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace CnGalWebSite.DataModel.Models
{
    public class FileManager
    {
        public int Id { get; set; }
        public long TotalSize { get; set; }

        public long UsedSize { get; set; }

        public ICollection<UserFile> UserFiles { get; set; }

        public string ApplicationUserId { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
    }

    public class UserFile
    {
        public int Id { get; set; }

        public string Sha1 { get; set; }

        /// <summary>
        /// 文件链接
        /// </summary>
        public string FileName { get; set; }

        public long? FileSize { get; set; }

        public DateTime UploadTime { get; set; }

        /// <summary>
        /// 音频长度
        /// </summary>
        public TimeSpan? Duration { get; set; }
        /// <summary>
        /// 文件类型
        /// </summary>
        public UploadFileType Type { get; set; }

        public FileManager FileManager { get; set; }

        public string UserId { get; set; }
    }

    public enum UploadFileType
    {
        [Display(Name = "图片")]
        Image,
        [Display(Name = "音频")]
        Audio
    }
}
