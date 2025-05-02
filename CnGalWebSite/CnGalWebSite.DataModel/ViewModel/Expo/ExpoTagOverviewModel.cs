using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Expo
{
    public class ExpoTagOverviewModel
    {
        public long Id { get; set; }

        public int Priority { get; set; }

        public string Name { get; set; }

        public bool Hidden { get; set; }

        public List<ExpoGameTagOverviewModel> Games { get; set; } = [];
    }

    public class ExpoTagViewModel : ExpoTagOverviewModel
    {

    }

    public class ExpoTagEditModel : ExpoTagOverviewModel
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
