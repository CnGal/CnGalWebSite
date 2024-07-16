using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.Model
{
    public class Carousel
    {
        public int Id { get; set; }

        public CarouselType Type { get; set; }

        public string Image { get; set; }

        public string Link { get; set; }

        public string Note { get; set; }

        public int Priority { get; set; }

    }

    public enum CarouselType
    {
        [Display(Name = "主页")]
        Home,
        [Display(Name = "专题页")]
        ThematicPage,
        [Display(Name = "活动")]
        Activity,
        [Display(Name = "PC底部横幅")]
        AdvertisingPC,
        [Display(Name = "APP底部横幅")]
        AdvertisingApp,
    }
}
