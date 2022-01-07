using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Votes
{
    public class UserVoteModel
    {
        public long VoteId { get; set; }

        public string UserId { get; set; }

        public List<long> VoteOptionIds { get; set; } = new List<long>();

        public bool IsAnonymous { get; set; }
    }
}
