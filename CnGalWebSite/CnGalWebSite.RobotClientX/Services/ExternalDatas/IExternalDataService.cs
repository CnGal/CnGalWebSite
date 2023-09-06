using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.RobotClientX.Services.ExternalDatas
{
    public interface IExternalDataService
    {
        Task<string> GetWeather();

        Task<string> GetArgValue(string name, string infor, long qq, Dictionary<string, string> adds);
    }
}
