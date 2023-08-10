using CnGalWebSite.RobotClientX.Models.ExternalDatas;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.RobotClientX.Services.ExternalDatas
{
    public class ExternalDataService:IExternalDataService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ExternalDataService> _logger;

        public ExternalDataService(HttpClient httpClient, IConfiguration configuration, ILogger<ExternalDataService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

        }

        public async Task< string> GetWeather()
        {
            var model = await _httpClient.GetFromJsonAsync<WeatherModel>(_configuration["WeatherUrl"]);

            if(model.Success!="1"||model.Result==null)
            {
                return null;
            }

            return $"{(model.Result.Temp_high != model.Result.Temp_low ? $"今日温度：{model.Result.Temperature}\n" : "")}当前温度：{model.Result.Temperature_curr}\n湿度：{model.Result.Humidity}\n{model.Result.Weather_curr}\n{model.Result.Wind}{model.Result.Winp}";
        }

    }
}
