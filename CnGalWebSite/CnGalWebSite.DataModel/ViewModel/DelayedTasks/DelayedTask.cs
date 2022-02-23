using System;

namespace CnGalWebSite.DataModel.ViewModel.DelayedTasks
{
    public class DelayedTask
    {
        public DelayedTaskType Type { get; set; }

        public DateTime CreateTime { get; set; }

        public string Context { get; set; }
    }

    public enum DelayedTaskType
    {
        DrawLottery
    }
}
