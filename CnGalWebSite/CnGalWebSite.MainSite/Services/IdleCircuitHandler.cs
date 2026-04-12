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
            catch (InvalidOperationException ex) when (ex.Message.Contains("SSR component ID"))
            {
                // 已知的良性异常：客户端持有缓存/过期的 SSR HTML，向新 Circuit 发送了旧的组件 ID
                // 常见触发场景：CDN 缓存页面、浏览器前进后退、Circuit 超时重连
                _logger.LogWarning("SSR 组件 ID 不匹配（已知良性） CircuitId={CircuitId}, Message={Message}",
                    context.Circuit.Id, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "线路内发生未处理异常 CircuitId={CircuitId}, ExceptionType={ExceptionType}",
                    context.Circuit.Id, ex.GetType().FullName);
                throw;
            }
        };
    }
}
