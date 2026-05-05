using CnGalWebSite.MainSite.Shared.Services;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace CnGalWebSite.MainSite.Services;

public sealed class KanbanEventCircuitHandler : CircuitHandler
{
    private readonly IKanbanEventService _kanbanEventService;

    public KanbanEventCircuitHandler(IKanbanEventService kanbanEventService)
    {
        _kanbanEventService = kanbanEventService;
    }

    public override Func<CircuitInboundActivityContext, Task> CreateInboundActivityHandler(
        Func<CircuitInboundActivityContext, Task> next)
    {
        return async context =>
        {
            _kanbanEventService.NotifyUserInteraction();
            await next(context);
        };
    }
}
