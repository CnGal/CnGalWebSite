using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Admin.Shared.Services
{
    public interface IHttpService
    {
        Task<TValue> GetAsync<TValue>(string url);

        Task<TValue> PostAsync<TModel, TValue>(string url, TModel model);

        bool IsAuth { get; set; }
    }
}
