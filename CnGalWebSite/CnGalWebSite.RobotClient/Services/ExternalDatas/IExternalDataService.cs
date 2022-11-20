using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.RobotClient.Services.ExternalDatas
{
    public interface IExternalDataService
    {
        Task<string> GetWeather();
    }
}
