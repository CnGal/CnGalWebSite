using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.RobotClientX.Models.ExternalDatas
{
    public class WeatherModel
    {
        public string Success { get; set; }

        public WeatherResult Result { get; set; }
    }

    public class WeatherResult
    {
        public string Temperature{ get; set; }
        public string Temperature_curr { get; set; }

        public string Humidity { get; set; }

        public string Weather { get; set; }
        public string Weather_curr { get; set; }

        public string Wind { get; set; }
        public string Winp { get; set; }

        public string Temp_low { get; set; }
        public string Temp_high { get; set; }


    }
}
