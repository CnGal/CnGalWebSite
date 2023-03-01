using CnGalWebSite.DataModel.ViewModel.OperationRecords;
using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Bookings
{
    public class BookingGameModel
    {
        public int GameId { get; set; }

        public bool IsBooking { get; set; }

        public DeviceIdentificationModel Identification { get; set; } = new DeviceIdentificationModel();

    }
}
