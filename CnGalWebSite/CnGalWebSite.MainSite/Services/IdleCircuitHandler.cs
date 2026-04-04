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

    public IdleCircuitHandler(ICircuitHandlerService circuitHandlerService)
    {
        _circuitHandlerService = circuitHandlerService;
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
        return context =>
        {
            _circuitHandlerService.ActiveCircuit(context.Circuit.Id);
            return next(context);
        };
    }
}
