using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.Helper.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.PostTools
{
    public class HostDataX
    {
        public readonly List<string> games = new List<string>();
        public readonly List<string> articles = new List<string>();
        private readonly SettingX setting;
        private readonly HttpClient client;

        public HostDataX(HttpClient _client, SettingX _setting)
        {
            client = _client;
            setting = _setting;
        }

        public async Task LoadAsync()
        {
            try
            {
                games.Clear();
                articles.Clear();

                games.AddRange(await client.GetFromJsonAsync<List<string>>(ToolHelper.WebApiPath + "api/entries/GetAllEntries/0"));
                //获取所有文章
                articles.AddRange(await client.GetFromJsonAsync<List<string>>(ToolHelper.WebApiPath + "api/articles/GetAllArticles"));

            }
            catch (Exception ex)
            {
                OutputHelper.PressError(ex, "获取所有词条文章名称失败");
            }

        }
    }
}
