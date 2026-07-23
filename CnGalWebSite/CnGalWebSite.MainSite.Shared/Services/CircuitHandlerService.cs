using System.Collections.Concurrent;

namespace CnGalWebSite.MainSite.Shared.Services;

public sealed class CircuitHandlerService : ICircuitHandlerService
{
    // 每个应用实例独立维护 Circuit 状态，使用线程安全字典处理实例内的并发读写
    private readonly ConcurrentDictionary<string, CircuitHandlerModel> _circuits = new();

    public void AddCircuit(string id)
    {
        _circuits[id] = new CircuitHandlerModel
        {
            Id = id,
            OpenTime = DateTime.Now,
            LastActiveTime = DateTime.Now,
            IsConnected = true
        };
    }

    public void RemoveCircuit(string id)
    {
        _circuits.TryRemove(id, out _);
    }

    public void ActiveCircuit(string id)
    {
        if (_circuits.TryGetValue(id, out var model))
        {
            model.LastActiveTime = DateTime.Now;
        }
    }

    public void ConnectCircuit(string id)
    {
        if (_circuits.TryGetValue(id, out var model))
        {
            model.ReConnectCount++;
            model.IsConnected = true;
        }
    }

    public void DisConnectCircuit(string id)
    {
        if (_circuits.TryGetValue(id, out var model))
        {
            model.IsConnected = false;
        }
    }

    public List<CircuitHandlerModel> GetItems()
    {
        return _circuits.Values.ToList();
    }
}
