using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Expo
{
    public class ExpoGameOverviewModel
    {
        public long Id { get; set; }

        public int Priority { get; set; }

        public int? GameId { get; set; }

        public bool Hidden { get; set; }

        public string GameName { get; set; }

        public List<ExpoGameTagOverviewModel> Tags { get; set; }=[];
    }

    public class ExpoGameViewModel : ExpoGameOverviewModel
    {
        public string Image { get; set; }

        public string Url { get; set; }
    }

    public class ExpoGameTagOverviewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class ExpoGameEditModel : ExpoGameOverviewModel
    {
        public Result Validate()
        {
            return new Result
            {
                Successful = true,
            };
        }
    }
}
