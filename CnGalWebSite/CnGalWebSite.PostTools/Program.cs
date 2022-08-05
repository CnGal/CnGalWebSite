// See https://aka.ms/new-console-template for more information

using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Accounts;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.Base;
using CnGalWebSite.DataModel.ViewModel.Files;
using CnGalWebSite.DataModel.ViewModel.HistoryData;
using CnGalWebSite.DataModel.ViewModel.Space;
using CnGalWebSite.Helper.Extensions;
using CnGalWebSite.Helper.Helper;
using HtmlAgilityPack;
using Markdig;
using PuppeteerSharp;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace CnGalWebSite.PostTools // Note: actual namespace depends on the project name.
{
    internal class Program
    {

        private static async Task Main(string[] args)
        {
            HttpClient _httpClient = new HttpClient();
            HtmlX _html = new HtmlX();
            SettingX _setting = new SettingX();

            ImageX _image = new ImageX(_httpClient, _setting);
            HistoryX _history = new HistoryX(_httpClient, _setting);
            HostDataX _hostData = new HostDataX(_httpClient, _setting);
            AccountX _account = new AccountX(_httpClient, _setting);
            MergeEntryX _MergeEntry = new MergeEntryX(_httpClient, _setting,_image,_hostData,_html,_history);
            OutlinkArticleX _outlinkArticle = new OutlinkArticleX(_httpClient, _setting, _image, _hostData, _html, _history);
            ImportPeripheryX _importPeriphery = new ImportPeripheryX(_httpClient, _setting, _image, _hostData, _html, _history);

            OutputHelper.Repeat();
            OutputHelper.WriteCenter("CnGal资料站 投稿工具 v0.8", 1.8);
            OutputHelper.Repeat();

            Console.WriteLine("-> 读取配置文件");
            _setting.Init();
            _setting.Load();
            Console.WriteLine("-> 读取图片缓存");
            _image.Load();

            Console.WriteLine("-> 读取已处理的数据");
            _history.Load();

            Console.WriteLine("-> 获取现有的词条和文章");
            await _hostData.LoadAsync();

            Console.WriteLine("-> 登入账户");

            await _account.LoginAsync();
            while (true)
            {
                OutputHelper.Repeat();
                Console.WriteLine("(1) 导入其他平台文章  (2) 合并词条  (3) 上传周边  (4) 退出登入");
                var text = Console.ReadLine();
                var index = 0;
                while (int.TryParse(text, out index) == false || index < 1 || index > 3)
                {
                    Console.WriteLine("请输入1,2或3");
                    text = Console.ReadLine();
                }

                switch (index)
                {
                    case 1:
                        await _outlinkArticle.ProcArticle();
                        break;
                    case 2:
                        await _MergeEntry.ProcMergeEntry();
                        break;
                    case 3:
                        await _importPeriphery.ProcImportPeriphery();
                        break;
                    case 4:
                        _account.Logout();
                        await _account.LoginAsync();
                        break;
                }
            }
        }
    }
}



