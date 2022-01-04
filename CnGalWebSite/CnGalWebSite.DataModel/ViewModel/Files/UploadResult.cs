using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.Model
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
        public string FileURL { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { get; set; }

    }
}
