using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Security.Policy;

namespace CnGalWebSite.Shared.Service
{
    public class HttpService: IHttpService
    {
        private readonly HttpClient _httpClient;

        public HttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TValue> GetAsync<TValue>(string url)
        {
            var client = GetClient();
            return await client.GetFromJsonAsync<TValue>(ToolHelper.WebApiPath + url, ToolHelper.options);
        }

        public async Task<TValue> PostAsync<TModel,TValue>(string url, TModel model)
        {
            var client = GetClient();
            var result = await client.PostAsJsonAsync(ToolHelper.WebApiPath + url, model);
            string jsonContent = result.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<TValue>(jsonContent, ToolHelper.options);
        }

        public HttpClient GetClient()
        {  
            return _httpClient;
        }
    }
}
