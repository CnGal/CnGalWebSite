using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CnGalWebSite.Components.Services
{
    public interface ISettingService
    {
        JsonSerializerOptions JsonOptions { get; }
    }
}
