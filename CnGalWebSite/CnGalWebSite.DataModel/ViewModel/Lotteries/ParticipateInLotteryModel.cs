using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.ViewModel.OperationRecords;
using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Lotteries
{
    public class ParticipateInLotteryModel
    {
        public long Id { get; set; }

        public DeviceIdentificationModel Identification { get; set; } = new DeviceIdentificationModel();

    }
}
