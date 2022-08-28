
using CnGalWebSite.DataModel.ViewModel.Files;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class ListFilesInforViewModel
    {
        public int All { get; set; }
    }
    public class ListFilesViewModel
    {
        public List<ListFileAloneModel> Files { get; set; }
    }
    public class ListFileAloneModel
    {
        [Display(Name = "Id")]
        public int Id { get; set; }
        [Display(Name = "链接")]
        public string FileName { get; set; }
        [Display(Name = "文件大小")]
        public long? FileSize { get; set; }
        [Display(Name = "上传时间")]
        public DateTime UploadTime { get; set; }

        [Display(Name = "上传的用户Id")]
        public string UserId { get; set; }
        [Display(Name = "上传的用户")]
        public string UserName { get; set; }

        [Display(Name = "音频长度")]
        public TimeSpan? AudioLength { get; set; }

        [Display(Name = "文件类型")]
        public UploadFileType Type { get; set; }

    }

    public class FilesPagesInfor
    {
        public CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions Options { get; set; }
        public ListFileAloneModel SearchModel { get; set; }

        public string Text { get; set; }
    }
}
