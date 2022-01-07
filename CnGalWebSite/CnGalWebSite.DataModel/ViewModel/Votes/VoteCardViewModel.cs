using CnGalWebSite.DataModel.ViewModel.Ranks;
using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Votes
{
    public class VoteCardViewModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string BriefIntroduction { get; set; }

        public string MainPicture { get; set; }

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }

        public long Count { get; set; }

        public List<VoteUserMinViewModel> Users { get; set; } = new List<VoteUserMinViewModel>();
    }

    public class VoteUserMinViewModel
    {
        public string UserId { get; set; }

        public string UserName { get; set; }

        public string Image { get; set; }
    }
}
