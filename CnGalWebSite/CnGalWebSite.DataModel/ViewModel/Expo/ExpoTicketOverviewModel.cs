using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Expo
{
    public class ExpoTicketOverviewModel
    {
        public long Id { get; set; }

        public long ActivityId { get; set; }

        public string ActivityName { get; set; }

        public string SeatInfo { get; set; }

        public string Nickname { get; set; }

        public int Number { get; set; }

        public DateTime CreateTime { get; set; }

        public bool Hidden { get; set; }

        public string Image { get; set; }
    }

    public class ExpoTicketViewModel : ExpoTicketOverviewModel
    {
        public ExpoActivityOverviewModel Activity { get; set; }
    }

    public class ExpoTicketEditModel : ExpoTicketOverviewModel
    {
        public Result Validate()
        {
            if (ActivityId <= 0)
            {
                return new Result { Successful = false, Error = "活动ID无效" };
            }

            return new Result { Successful = true };
        }
    }
}
