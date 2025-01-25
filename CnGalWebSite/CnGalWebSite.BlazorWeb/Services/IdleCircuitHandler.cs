using CnGalWebSite.BlazorWeb.Services;
using CnGalWebSite.Shared.Service;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.Options;
using Timer = System.Timers.Timer;

namespace CnGalWebSite.BlazorWeb.Services
{
    public sealed class IdleCircuitHandler : CircuitHandler
    {
        private readonly ILogger _logger;
        private readonly ICircuitHandlerService _circuitHandlerService;

        public IdleCircuitHandler(ILogger<IdleCircuitHandler> logger, ICircuitHandlerService circuitHandlerService)
        {
            _logger = logger;
            _circuitHandlerService = circuitHandlerService;
        }

        public override Task OnCircuitOpenedAsync(Circuit circuit,
            CancellationToken cancellationToken)
        {
            _circuitHandlerService.AddCircuit(circuit.Id);
            return Task.CompletedTask;
        }

        public override Task OnCircuitClosedAsync(Circuit circuit,
        CancellationToken cancellationToken)
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

}

