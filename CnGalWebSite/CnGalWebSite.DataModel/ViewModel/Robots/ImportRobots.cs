using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Robots
{
    public class ImportRobotsModel
    {
        [Display(Name = "Json字符串")]
        public string Value { get; set; }
    }
}
