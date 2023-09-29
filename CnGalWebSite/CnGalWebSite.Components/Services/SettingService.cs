using CnGalWebSite.Components.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CnGalWebSite.Components.Services
{
    public class SettingService:ISettingService
    {
        private JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };


        public SettingService() {

            _jsonOptions.Converters.Add(new DateTimeConverterUsingDateTimeParse());
            _jsonOptions.Converters.Add(new DateTimeConverterUsingDateTimeNullableParse());
            _jsonOptions.Converters.Add(new JsonStringEnumConverter());
        }

        public JsonSerializerOptions JsonOptions => _jsonOptions;
    }
}
