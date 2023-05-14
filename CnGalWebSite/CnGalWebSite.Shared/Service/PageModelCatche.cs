using CnGalWebSite.Core.Services;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.Shared.Pages.Normal.Admin;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace CnGalWebSite.Shared.Service
{
    public class PageModelCatche<TModel> : IDisposable,IPageModelCatche<TModel> where TModel : class
    {
        private readonly IHttpService _httpService;
        private readonly PersistentComponentState ApplicationState;
        private readonly IServiceProvider _serviceProvider;
        /// <summary>
        /// 请求的URL
        /// </summary>
        private string _baseUrl { get; set; } = string.Empty;
        private bool _useNewtonsoft { get; set; }
        private string _token { get; set; }

        /// <summary>
        /// 缓存列表
        /// </summary>
        private Dictionary<string, TModel> _catches { get; set; } = new Dictionary<string, TModel>();

        private PersistingComponentStateSubscription persistingSubscription;

        public PageModelCatche(HttpClient httpClient, IServiceProvider serviceProvider, IHttpService httpService)
        {
            _httpService = httpService;
            _serviceProvider = serviceProvider;
            _token =typeof( TModel).ToString();
            _baseUrl = ToolHelper.WebApiPath;

            if (ToolHelper.IsSSR)
            {
                ApplicationState = (PersistentComponentState)_serviceProvider.GetService(typeof(PersistentComponentState));
                persistingSubscription = ApplicationState.RegisterOnPersisting(PersistData);
            }
        }

        public void Init(string name, string baseUrl, bool useNewtonsoft = false)
        {
            _baseUrl = baseUrl ?? string.Empty ;
            _useNewtonsoft = useNewtonsoft;
        }

        string GetUrl(string apiUrl) => apiUrl.Contains("http://") ? apiUrl : _baseUrl + apiUrl;

        private async Task<TModel> GetCacheFromMemory(string apiUrl)
        {
            //判断是否两次请求同一个API
            //是则清除缓存
            //if (_lastRequestUrl == apiUrl && _catches.Any(s => s.Key == _baseUrl + apiUrl) && noRefresh == false)
            //{
            //    _catches.Remove(_baseUrl + apiUrl);
            //}

            var url = GetUrl(apiUrl);

            if (_catches.Any(s => s.Key == url))
            {
                return _catches[url];
            }
            else
            {
                //获取数据
                var client = await _httpService.GetClientAsync();
                TModel temp = null;
                if (_useNewtonsoft)
                {
                    var str = await client.GetStringAsync(url);
                    var obj = Newtonsoft.Json.Linq.JObject.Parse(str);
                    temp = obj.ToObject<TModel>();
                }
                else
                {
                    temp = await client.GetFromJsonAsync<TModel>(url, ToolHelper.options);
                }

                return temp;
            }
        }

        public void Clean(string apiUrl)
        {
            var url = GetUrl(apiUrl);

            _ = _catches.Remove(url);
        }
        public bool Check(string apiUrl)
        {
            var url = GetUrl(apiUrl);
            return _catches.Any(s => s.Key == url);
        }
        public void Clean()
        {
            _catches.Clear();
        }

        public async Task<TModel> GetCache(string apiUrl)
        {
            var url = GetUrl(apiUrl);

            if (ApplicationState != null)
            {
                if (!ApplicationState.TryTakeFromJson<Dictionary<string, TModel>>(_token, out var restored))
                {

                }
                else
                {
                    _catches = restored!;
                }
            }

           var temp = await GetCacheFromMemory(apiUrl);

            //保存数据
            if (_catches.Any(s => s.Key == url))
            {
                _catches[url] = temp;
            }
            else
            {
                _catches.Add(url, temp);
            }



            return temp;
        }

        private async Task PersistData()
        {
            try
            {
                ApplicationState.PersistAsJson(_token, _catches);
            }
            catch
            {

            }
        }

        void IDisposable.Dispose()
        {
            persistingSubscription.Dispose();
        }
    }
}
