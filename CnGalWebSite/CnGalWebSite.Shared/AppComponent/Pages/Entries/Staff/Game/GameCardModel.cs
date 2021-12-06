using System.Collections.Generic;

namespace CnGalWebSite.Shared.AppComponent.Pages.Entries.Staff.Game
{
    public class GameCardModel
    {
        public string Name { get; set; }
        public string BriefIntroduction { get; set; }
        public string Link { get; set; }

        public List<string> Positions { get; set; }

        public string Image { get; set; }
    }
}
