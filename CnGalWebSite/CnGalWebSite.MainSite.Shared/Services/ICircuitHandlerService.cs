

namespace CnGalWebSite.MainSite.Shared.Services;

public class CircuitHandlerModel
{
    public string Id { get; set; } = null!;
    public DateTime OpenTime { get; set; }
    public DateTime LastActiveTime { get; set; }
    public bool IsConnected { get; set; }
    public int ReConnectCount { get; set; }
}

public interface ICircuitHandlerService
{
    void AddCircuit(string id);
    void RemoveCircuit(string id);
    void ActiveCircuit(string id);
    void ConnectCircuit(string id);
    void DisConnectCircuit(string id);
    List<CircuitHandlerModel> GetItems();
}
