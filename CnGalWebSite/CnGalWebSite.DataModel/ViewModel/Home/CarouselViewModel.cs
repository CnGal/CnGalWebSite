using CnGalWebSite.DataModel.Model;

namespace CnGalWebSite.DataModel.ViewModel.Home
{
    public class CarouselViewModel
    {
        public string Image { get; set; }

        public string Link { get; set; }

        public int Priority { get; set; }

        public string Note { get; set; }

        public CarouselType Type { get; set; }
    }
}
