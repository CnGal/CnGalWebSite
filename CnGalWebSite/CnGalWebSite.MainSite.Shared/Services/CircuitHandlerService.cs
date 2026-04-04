

namespace CnGalWebSite.MainSite.Shared.Services;

public sealed class CircuitHandlerService : ICircuitHandlerService
{
    private readonly Dictionary<string, CircuitHandlerModel> _circuits = new();

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
        _circuits.Remove(id);
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
