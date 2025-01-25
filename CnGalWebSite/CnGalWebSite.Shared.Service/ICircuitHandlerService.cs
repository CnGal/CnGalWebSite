using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Shared.Service
{
    public interface ICircuitHandlerService
    {
        void AddCircuit(string id);
        void RemoveCircuit(string id);
        void ActiveCircuit(string id);
        void ConnectCircuit(string id);
        void DisConnectCircuit(string id);
        List<CircuitHandlerModel> GetItems();
    }
}
