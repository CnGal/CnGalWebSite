using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Home
{
    public class CarouselOverviewModel
    {
        public int Id { get; set; }

        public CarouselType Type { get; set; }

        public string Image { get; set; }

        public string Link { get; set; }

        public string Note { get; set; }

        public int Priority { get; set; }
    }

    public class CarouselEditModel: CarouselOverviewModel
    {

    }
}
