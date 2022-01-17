using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Ranks;

using System;
using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class ExaminedNormalListModel
    {
        public long Id { get; set; }

        public ExaminedNormalListModelType Type { get; set; }

        public DateTime ApplyTime { get; set; }

        public DateTime? PassedTime { get; set; }

        public string RelatedId { get; set; }

        public string RelatedName { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public string UserImage { get; set; }

        public Operation Operation { get; set; }

        public bool? IsPassed { get; set; }

        public List<RankViewModel> Ranks { get; set; }

    }

    public enum ExaminedNormalListModelType
    {
        Entry,
        Article,
        User,
        Tag,
        Comment,
        Disambig,
        Periphery
    }
}
