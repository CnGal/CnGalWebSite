using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.PublicToolbox.Models
{
    public class RepostVideoModel : ToolTaskBase
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string Type { get; set; }

        public string Url { get; set; }
        /// <summary>
        /// 是否为用户本人创作
        /// </summary>
        public bool IsCreatedByCurrentUser { get; set; }

        public RepostVideoModel()
        {
            TotalTaskCount = 5;
        }
    }

    public enum RepostVideoType
    {
        Bilibili
    }
}
