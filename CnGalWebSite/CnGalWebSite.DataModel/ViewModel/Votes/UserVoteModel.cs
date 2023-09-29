using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.ViewModel.OperationRecords;
using System.Collections.Generic;

namespace CnGalWebSite.DataModel.ViewModel.Votes
{
    public class UserVoteModel
    {
        public long VoteId { get; set; }

        public List<long> VoteOptionIds { get; set; } = new List<long>();

        public bool IsAnonymous { get; set; }

        public DeviceIdentificationModel Identification { get; set; } = new DeviceIdentificationModel();

    }
}
