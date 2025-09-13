using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Expo
{
    public class ExpoActivityOverviewModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string ForegroundImage { get; set; }

        public string BackgroundImage { get; set; }

        public string Description { get; set; }

        public int Priority { get; set; }

        public bool Hidden { get; set; }

        public DateTime CreateTime { get; set; }

        public int TicketCount { get; set; }
    }

    public class ExpoActivityViewModel : ExpoActivityOverviewModel
    {
        public List<ExpoTicketOverviewModel> Tickets { get; set; } = new List<ExpoTicketOverviewModel>();
    }

    public class ExpoActivityEditModel : ExpoActivityOverviewModel
    {
        public List<ExpoTicketEditModel> Tickets { get; set; } = new List<ExpoTicketEditModel>();

        public Result Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return new Result { Successful = false, Error = "活动名称不能为空" };
            }

            if (string.IsNullOrWhiteSpace(ForegroundImage))
            {
                return new Result { Successful = false, Error = "前景图片不能为空" };
            }

            if (string.IsNullOrWhiteSpace(BackgroundImage))
            {
                return new Result { Successful = false, Error = "背景图片不能为空" };
            }

            return new Result { Successful = true };
        }
    }
}
