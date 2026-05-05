#nullable enable
using System.Collections.Generic;

namespace CnGalWebSite.MainSite.Shared.Services.KanbanModels
{
    public class KanbanMotionGroupModel
    {
        public required string Name { get; set; }
        public required string DisplayName { get; set; }
        public List<KanbanMotionModel> Motions { get; set; } = new();
    }

    public class KanbanMotionModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}
