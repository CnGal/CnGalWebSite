using CnGalWebSite.DataModel.Model;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.PlayedGames
{
    public class EditGameRecordModel
    {

        public int GameId { get; set; }

        public bool IsHidden { get; set; }

        [Display(Name ="类型")]
        public PlayedGameType Type { get; set; }

        [Display(Name = "感想")]
        public string PlayImpressions { get; set; }
    }
}
