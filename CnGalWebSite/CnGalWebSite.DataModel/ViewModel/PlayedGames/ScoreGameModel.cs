using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.PlayedGames
{
    public class ScoreGameModel
    {
        public int GameId { get; set; }

        [Range(0, 5, ErrorMessage = "请在1-5范围评分")]
        public int CVSocre { get; set; }
        [Range(0, 5, ErrorMessage = "请在1-5范围评分")]
        public int ShowSocre { get; set; }
        [Range(0, 5, ErrorMessage = "请在1-5范围评分")]
        public int SystemSocre { get; set; }
        [Range(0, 5, ErrorMessage = "请在1-5范围评分")]
        public int PaintSocre { get; set; }
        [Range(0, 5, ErrorMessage = "请在1-5范围评分")]
        public int ScriptSocre { get; set; }
    }
}
