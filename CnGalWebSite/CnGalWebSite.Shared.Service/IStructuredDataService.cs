using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Shared.Service
{
    public interface IStructuredDataService
    {
        EventCallback Refresh { get; set; }

        string Data { get; set; }

        Task SetStructuredData(string data);

        Task SetStructuredData(List<string> urls);
    }
}
