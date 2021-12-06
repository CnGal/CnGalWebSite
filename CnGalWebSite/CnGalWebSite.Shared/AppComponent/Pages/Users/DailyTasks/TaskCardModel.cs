using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;

namespace CnGalWebSite.Shared.AppComponent.Pages.Users.DailyTasks
{
    public class TaskCardModel
    {
        public string Icon { get; set; }

        public string Title { get; set; }

        public string Text { get; set; }

        public bool IsCompleted { get; set; }

        public Color ButtonColor { get; set; }
        public string IconColor { get; set; }

        public string ButtonText { get; set; }

        public EventCallback OnCompleting { get; set; }

    }
}
