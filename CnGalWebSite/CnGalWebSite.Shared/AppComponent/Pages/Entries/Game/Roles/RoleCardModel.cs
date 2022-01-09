using System.Collections.Generic;

namespace CnGalWebSite.Shared.AppComponent.Pages.Entries.Game.Roles
{
    public class RoleCardModel
    {
        public string Image { get; set; }

        public string Name { get; set; }

        public List<string> CVs { get; set; }

        public int EntryId { get; set; }
    }
}
