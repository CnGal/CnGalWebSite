using Microsoft.AspNetCore.Components;

namespace CnGalWebSite.Shared.AppComponent.Normal.Links
{
    public class IconTextModel
    {
        public string Icon { get; set; }

        public string Text { get; set; }

        public string Url { get; set; }


        public bool IsSplitLine { get; set; }

        public bool IsOutLink { get; set; }

        public EventCallback OnClick { get; set; }
    }
}
