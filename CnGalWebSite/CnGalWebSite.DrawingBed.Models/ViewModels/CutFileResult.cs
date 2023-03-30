using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DrawingBed.Models.ViewModels
{
    public class CutFileResult
    {
        /// <summary>
        /// 音频长度
        /// </summary>
        public TimeSpan? Duration { get; set; }
        /// <summary>
        /// 文件长度
        /// </summary>
        public long? FileSize { get; set; }
        /// <summary>
        /// 处理过的文件url
        /// </summary>
        public string FileURL { get; set; }
    }
}
