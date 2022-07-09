using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Space
{
    public class SpaceViewCacheModel
    {
        public int TabIndex { get; set; } = 1;

        public int ExaminesCurrentPage { get; set; } = 1;

        public int FavoriteObjectsCurrentPage { get; set; } = 1;
        public long PageFavoriteFolderId { get; set; }

        public long ViewOnFavoriteFolderId { get; set; }

        public string ViewOnFavoriteFolderName { get; set; }

        public string UserId { get; set; }
    }
}
