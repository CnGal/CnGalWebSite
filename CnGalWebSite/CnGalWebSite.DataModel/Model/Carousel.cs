using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.Model
{
    public class Carousel
    {
        public int Id { get; set; }

        public string Image { get; set; }

        public string Link { get; set; }

        public int Priority { get; set; } = 0;

    }
}
