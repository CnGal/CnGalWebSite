using CnGalWebSite.MainSite.Shared.Services;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CnGalWebSite.MainSite.Services;

public sealed class IdleCircuitHandler : CircuitHandler
{
    private readonly ICircuitHandlerService _circuitHandlerService;
    private readonly ILogger<IdleCircuitHandler> _logger;

    public IdleCircuitHandler(ICircuitHandlerService circuitHandlerService, ILogger<IdleCircuitHandler> logger)
    {
        _circuitHandlerService = circuitHandlerService;
        _logger = logger;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _circuitHandlerService.AddCircuit(circuit.Id);
        return Task.CompletedTask;
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _circuitHandlerService.RemoveCircuit(circuit.Id);
        return Task.CompletedTask;
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _circuitHandlerService.DisConnectCircuit(circuit.Id);
        return Task.CompletedTask;
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _circuitHandlerService.ConnectCircuit(circuit.Id);
        return Task.CompletedTask;
    }

    public override Func<CircuitInboundActivityContext, Task> CreateInboundActivityHandler(
        Func<CircuitInboundActivityContext, Task> next)
    {
        return async context =>
        {
            _circuitHandlerService.ActiveCircuit(context.Circuit.Id);
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "线路内发生未处理异常 CircuitId={CircuitId}", context.Circuit.Id);
                throw;
            }
        };
    }
}
