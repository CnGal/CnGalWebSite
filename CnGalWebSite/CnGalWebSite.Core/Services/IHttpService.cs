using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Core.Services
{
    public interface IHttpService
    {
        Task<TValue> GetAsync<TValue>(string url);

        Task<TValue> PostAsync<TModel, TValue>(string url, TModel model);

        Task<HttpClient> GetClientAsync();
    }
}
