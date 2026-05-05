namespace CnGalWebSite.MainSite.Shared.Services;

public interface IKanbanCommodityOwnershipService
{
    event Action? OwnershipChanged;

    void NotifyOwnershipChanged();
}
