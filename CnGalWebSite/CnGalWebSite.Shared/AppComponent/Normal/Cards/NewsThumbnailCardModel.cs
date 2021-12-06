using System;

namespace CnGalWebSite.Shared.AppComponent.Normal.Cards
{
    public class NewsThumbnailCardModel
    {
        public string Image { get; set; }
        public string Title { get; set; }
        public int GroupId { get; set; }

        public string Type { get; set; }
        public string Text { get; set; }
        public DateTime Time { get; set; }

        /// <summary>
        /// 跳转链接
        /// </summary>
        public string Url { get; set; }

        public bool IsNeedLayout { get; set; } = true;

        public bool IsOutLink { get; set; }

    }
}
