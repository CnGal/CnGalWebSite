﻿using System;
namespace CnGalWebSite.DataModel.Model
{
    [Obsolete("已移除")]
    public class ErrorCount
    {
        public long Id { get; set; }

        public string Text { get; set; }

        public int Count { get; set; }

        public DateTime LastUpdateTime { get; set; }
    }
}
