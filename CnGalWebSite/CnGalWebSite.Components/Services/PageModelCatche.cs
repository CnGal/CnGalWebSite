using CnGalWebSite.Components.Services;
using CnGalWebSite.Core.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CnGalWebSite.Components.Service
{
    public class PageModelCatche<TModel> : IDisposable, IPageModelCatche<TModel> where TModel : class
    {
        private readonly IHttpService _httpService;
        private readonly PersistentComponentState ApplicationState;
        private readonly IServiceProvider _serviceProvider;
        /// <summary>
        /// 请求的URL
        /// </summary>
        private string _baseUrl { get; set; } = string.Empty;
        private string _token { get; set; }

        /// <summary>
        /// 缓存列表
        /// </summary>
        private Dictionary<string, TModel> _catches { get; set; } = new Dictionary<string, TModel>();


        private PersistingComponentStateSubscription persistingSubscription;

        public PageModelCatche(IServiceProvider serviceProvider, IHttpService httpService, IConfiguration configuration, PersistentComponentState _applicationState)
        {
            _httpService = httpService;
            _serviceProvider = serviceProvider;

            _token = typeof(TModel).ToString();
            _baseUrl = configuration["WebApiPath"];


            //ApplicationState = _applicationState;
            //persistingSubscription = ApplicationState.RegisterOnPersisting(PersistData,);

        }

        public void Init(string name, string baseUrl)
        {
            _baseUrl = baseUrl ?? string.Empty;
        }

        string GetUrl(string apiUrl) => apiUrl.Contains("://") ? apiUrl : _baseUrl + apiUrl;

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
                TModel temp = await _httpService.GetAsync<TModel>(url);

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

        private Task PersistData()
        {
            ApplicationState.PersistAsJson(_token, _catches);
            return Task.CompletedTask;
        }

        void IDisposable.Dispose()
        {
            persistingSubscription.Dispose();
        }
    }
}
