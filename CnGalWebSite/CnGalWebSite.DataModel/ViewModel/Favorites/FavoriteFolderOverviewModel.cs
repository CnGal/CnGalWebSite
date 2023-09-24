using System;
using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Favorites
{
    public class FavoriteFolderOverviewModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public bool IsDefault { get; set; }
        /// <summary>
        /// 是否向他人公开
        /// </summary>
        public bool ShowPublicly { get; set; }

        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool IsHidden { get; set; }

        public string MainImage { get; set; }

        public string BriefIntroduction { get; set; }

        public DateTime CreateTime { get; set; }

        public long Count { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }
    }

}
