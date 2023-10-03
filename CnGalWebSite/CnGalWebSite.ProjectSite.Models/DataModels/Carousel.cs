using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.ProjectSite.Models.DataModels
{
    public class Carousel: BaseModel
    {
        public long Id { get; set; }

        public string Image { get; set; }

        public string Link { get; set; }

        public string Note { get; set; }

    }
}
