using Microsoft.AspNetCore.Components;

namespace CnGalWebSite.Shared.AppComponent.Normal.Tabs
{
    public class TabViewItemModel
    {
        public EventCallback<int> OnTabClick { get; set; }

        public string Icon { get; set; }
        public string Text { get; set; }
        public int Index { get; set; }

        public int DefaultIndex { get; set; }

        public int RandomIndex { get; set; }

        public string ClassNames { get; set; }

        public TableItemType Type { get; set; }


    }

    public enum TableItemType
    {
        Bottom,
        Pivot,
        Badge
    }
}
