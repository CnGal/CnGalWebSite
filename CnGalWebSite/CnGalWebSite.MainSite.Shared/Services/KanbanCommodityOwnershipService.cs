namespace CnGalWebSite.MainSite.Shared.Services;

public sealed class KanbanCommodityOwnershipService : IKanbanCommodityOwnershipService
{
    public event Action? OwnershipChanged;

    public void NotifyOwnershipChanged()
    {
        OwnershipChanged?.Invoke();
    }
}
