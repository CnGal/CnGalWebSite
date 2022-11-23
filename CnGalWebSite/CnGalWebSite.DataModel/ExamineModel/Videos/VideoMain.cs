using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ExamineModel.Videos
{
    public class VideoMain
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }

        public string BriefIntroduction { get; set; }

        public string MainPicture { get; set; }

        public string BackgroundPicture { get; set; }

        public string SmallBackgroundPicture { get; set; }

        public string OriginalAuthor { get; set; }

        public DateTime PubishTime { get; set; }

        public string Type { get; set; }

        /// <summary>
        /// 版权
        /// </summary>
        public CopyrightType Copyright { get; set; }
        /// <summary>
        /// 时长
        /// </summary>
        public TimeSpan Duration { get; set; }
        /// <summary>
        /// 是否为互动视频
        /// </summary>
        public bool IsInteractive { get; set; }
        /// <summary>
        /// 是否为用户本人创作
        /// </summary>
        public bool IsCreatedByCurrentUser { get; set; }

    }
}
