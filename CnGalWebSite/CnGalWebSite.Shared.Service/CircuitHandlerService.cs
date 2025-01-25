
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.Shared.Service
{
    public sealed class CircuitHandlerService :ICircuitHandlerService
    {
        private readonly ILogger logger;
        private Dictionary<string, CircuitHandlerModel> _circuits = [];

        public CircuitHandlerService(ILogger<CircuitHandlerService> logger)
        {
            this.logger = logger;
        }

        public void AddCircuit(string id)
        {
            _circuits[id] = new CircuitHandlerModel
            {
                Id = id,
                OpenTime = DateTime.Now,
            };
        }

        public void RemoveCircuit(string id)
        {
            _circuits.Remove(id);
        }

        public void ActiveCircuit(string id)
        {
            if (_circuits.TryGetValue(id, out CircuitHandlerModel model))
            {
                model.LastActiveTime = DateTime.Now;
            }
        }

        public void ConnectCircuit(string id)
        {
            if (_circuits.TryGetValue(id, out CircuitHandlerModel model))
            {
                model.ReConnectCount++;
                model.IsConnected = true;
            }
        }

        public void DisConnectCircuit(string id)
        {
            if (_circuits.TryGetValue(id, out CircuitHandlerModel model))
            {
                model.IsConnected = false;
            }
        }

        public List<CircuitHandlerModel> GetItems()
        {
            return _circuits.Values.ToList();
        }
    }

    public class CircuitHandlerModel
    {
        public string Id { get; set; }

        public DateTime OpenTime { get; set; }

        public DateTime LastActiveTime { get; set; }

        public bool IsConnected { get; set; }

        public int ReConnectCount {  get; set; }
    }
}

