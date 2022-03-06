using BootstrapBlazor.Components;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Policy;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace CnGalWebSite.Shared.Service
{
    public class PageModelCatche<TModel> : IPageModelCatche<TModel> where TModel : class
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// 请求的URL
        /// </summary>
        private string _baseUrl { get; set; } = string.Empty;
        private bool _useNewtonsoft { get; set; }

        private string _lastRequestUrl { get; set; } = string.Empty;
        /// <summary>
        /// 缓存列表
        /// </summary>
        private Dictionary<string, TModel> _catches { get; set; } = new Dictionary<string, TModel>();

        public PageModelCatche(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public void Init(string baseUrl,bool useNewtonsoft = false)
        {
            _baseUrl = baseUrl ?? string.Empty;
            _useNewtonsoft = useNewtonsoft;
        }

        public async Task<TModel> GetCatche(string apiUrl, bool noRefresh = false)
        {
            //判断是否两次请求同一个API
            //是则清除缓存
            if (_lastRequestUrl == apiUrl && _catches.Any(s => s.Key == _baseUrl + apiUrl) && noRefresh == false)
            {
                _catches.Remove(_baseUrl + apiUrl);
            }

            if (_catches.Any(s => s.Key == _baseUrl + apiUrl))
            {
                _lastRequestUrl = apiUrl;
                return _catches[_baseUrl + apiUrl];
            }
            else
            {
                //获取数据
                TModel temp = null;
                if(_useNewtonsoft)
                {
                    var str = await _httpClient.GetStringAsync(_baseUrl + apiUrl);
                    Newtonsoft.Json.Linq.JObject obj = Newtonsoft.Json.Linq.JObject.Parse(str);
                    temp = obj.ToObject<TModel>();
                }
                else
                {
                    temp=  await _httpClient.GetFromJsonAsync<TModel>(_baseUrl + apiUrl);
                }

                //保存数据
                if (_catches.Any(s => s.Key == _baseUrl + apiUrl))
                {
                    _catches[_baseUrl + apiUrl] = temp;
                }
                else
                {
                    _catches.Add(_baseUrl + apiUrl, temp);
                }

                _lastRequestUrl = apiUrl;
                return temp;
            }
        }

        public void Clean(string apiUrl)
        {
            _catches.Remove(_baseUrl + apiUrl);
        }
        public void Clean()
        {
            _catches.Clear();
        }
    }
}
