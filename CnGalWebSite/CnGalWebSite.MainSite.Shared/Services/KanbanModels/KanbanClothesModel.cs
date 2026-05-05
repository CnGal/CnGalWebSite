#nullable enable

namespace CnGalWebSite.MainSite.Shared.Services.KanbanModels
{
    public class KanbanClothesModel
    {
        public required string Name { get; set; }
        public required string DisplayName { get; set; }
        public string? IconUrl { get; set; }
    }

    public class KanbanStockingsModel : KanbanClothesModel;

    public class KanbanShoesModel : KanbanClothesModel;
}
