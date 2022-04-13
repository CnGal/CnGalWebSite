﻿using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Entries;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Search;
using CnGalWebSite.APIServer.Application.SteamInfors;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.Models;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Home;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.Helper.Extensions;
using Gt3_server_csharp_aspnetcoremvc_bypass.Controllers.Sdk;
using Markdig;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NETCore.MailKit.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.Entities.Menu;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.WeiXin
{
    public class WeiXinService:IWeiXinService
    {
        private readonly IConfiguration _configuration;
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IArticleService _articleService;
        private readonly IAppHelper _appHelper;
        private readonly ISearchHelper _searchHelper;
        private readonly ISteamInforService _steamService;

        public WeiXinService(IConfiguration configuration, IRepository<Entry, int> entryRepository, IRepository<Article, long> articleRepository, IArticleService articleService, IAppHelper appHelper, ISteamInforService steamService, ISearchHelper searchHelper)
        {
            _configuration = configuration;
            _entryRepository = entryRepository;
            _articleRepository = articleRepository;
            _articleService = articleService;
            _appHelper = appHelper;
            _steamService = steamService;
            _searchHelper=searchHelper;
        }

        public void CreateMenu()
        {
            ButtonGroup bg = new ButtonGroup();
            //只存在一级菜单
            bg.button.Add(new SingleClickButton()
            {
                name = "随机推荐",
                key = "Click_Random",
            });

            //存在二级菜单
            var subButton = new SubButton()
            {
                name = "获取情报",
            };
            subButton.sub_button.Add(new SingleClickButton()
            {
                key = "Click_Newest_News",
                name = "最新动态"
            });
            subButton.sub_button.Add(new SingleClickButton()
            {
                key = "Click_Newest_PubishGame",
                name = "近期新作"
            });
            subButton.sub_button.Add(new SingleClickButton()
            {
                key = "Click_Newest_UnPublishGame",
                name = "即将发售"
            });
            subButton.sub_button.Add(new SingleClickButton()
            {
                key = "Click_Newest_EditGame",
                name = "最新编辑"
            });

            bg.button.Add(subButton);

            //存在二级菜单
            subButton = new SubButton()
            {
                name = "关于",
            };
            subButton.sub_button.Add(new SingleViewButton()
            {
                url = "https://www.cngal.org/about",
                name = "关于资料站"
            });
            subButton.sub_button.Add(new SingleClickButton()
            {
                key = "Click_About_Usage",
                name = "戳戳看板娘"
            });

            bg.button.Add(subButton);

            var result = CommonApi.CreateMenu(null, bg);

        }

        public string GetAboutUsage()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("o((>ω< ))o");
            sb.AppendLine("这里是看板娘哦~~~欸嘿嘿~~~");
            sb.AppendLine("你问我能干什么？");
            sb.AppendLine("抱歉啦，毕竟刚刚从沉睡中醒来，还不是很熟悉这个世界呢");
            sb.AppendLine("不过数据库还是能连接上的，试试下面的指令吧~");
            sb.AppendLine();
            sb.AppendLine("【随机推荐】【最新动态】【近期新作】【即将发售】【最新编辑】");
            sb.AppendLine("【词条+Id】");
            sb.AppendLine("例如：词条 1");
            sb.AppendLine("【搜索+查找的内容】");
            sb.Append("例如：搜索 CnGal");

            return sb.ToString();
        }

        public async Task<string> GetSearchResults(string text)
        {
            var result = await _searchHelper.QueryAsync(1, 6, text, "全部", null, QueryType.Page);
            StringBuilder sb = new StringBuilder();

            if (result.TotalCount == 0)
            {
                return "呜~~~ 找不到喵~";
            }

            foreach(var item in result.Data)
            {
                if(item.entry!=null)
                {
                    sb.AppendLine($"【{item.entry.Type.GetDisplayName()}】 <a href=\"https://www.cngal.org/entries/index/{item.entry.Id}\">{item.entry.DisplayName}</a>");
                }
                else if (item.article != null)
                {
                    sb.AppendLine($"【{item.article.Type.GetDisplayName()}】 <a href=\"https://www.cngal.org/articles/index/{item.article.Id}\">{item.article.DisplayName}</a>");
                }
                else if (item.tag != null)
                {
                    sb.AppendLine($"【标签】 <a href=\"https://www.cngal.org/tags/index/{item.tag.Id}\">{item.tag.Name}</a>");

                }
                else if (item.periphery != null)
                {
                    sb.AppendLine($"【周边】 <a href=\"https://www.cngal.org/peripheries/index/{item.periphery.Id}\">{item.periphery.Name}</a>");
                }
            }

            return sb.ToString();
        }

        public async Task<string> GetRandom(bool plainText = false)
        {
            var entryIds = await _entryRepository.GetAll().AsNoTracking()
                .Where(s => s.Type == EntryType.Game && string.IsNullOrWhiteSpace(s.Name) == false && s.IsHidden != true)
                .Select(s => s.Id).ToListAsync();

            var index = new Random().Next(0, entryIds.Count - 1);
            return await GetEntryInfor(entryIds[index], plainText);
        }

        public async Task<string> GetEntryInfor(int id,bool plainText=false)
        {
            var entry = await _entryRepository.GetAll().AsNoTracking()
                .Include(s => s.Information)
                .Include(s => s.EntryRelationFromEntryNavigation).ThenInclude(s => s.ToEntryNavigation)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (entry == null)
            {
                return "呜~~~ 搜索不到欸~";
            }

            var model = await _appHelper.GetEntryInforTipViewModel(entry);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"{model.Type.GetDisplayName()} - <a href=\"https://www.cngal.org/entries/index/{model.Id}\">{model.Name}</a>");
            if (string.IsNullOrWhiteSpace(model.BriefIntroduction) == false)
            {
                sb.AppendLine($"{model.BriefIntroduction.Abbreviate(23)}");
            }


            if (model.Type == EntryType.Game)
            {

                var steamIdStr = entry.Information.FirstOrDefault(s => s.Modifier == "基本信息" && s.DisplayName == "Steam平台Id")?.DisplayValue;
                int steamId = 0;
                if (steamIdStr != null && int.TryParse(steamIdStr, out steamId))
                {
                    sb.AppendLine();

                    var steam = await _steamService.GetSteamInforAsync(steamId, model.Id);
                    if (steam.PriceNow > 0)
                    {
                        sb.AppendLine($"当前价格：{steam.PriceNowString}{ (steam.CutNow == 0 ? "" : " - 折扣 " + steam.CutNow + "%")}");

                        if (steam.CutLowest > 0)
                        {
                            sb.AppendLine($"历史最低：{ steam.PriceLowestString}");
                        }
                    }
                    else if (steam.PriceNow == 0)
                    {
                        sb.AppendLine($"当前价格：¥ 0.00 - Free");
                    }
                    else if (steam.PriceNow == -1)
                    {
                        sb.AppendLine($"未发售");


                    }
                    else if (steam.PriceNow == -2)
                    {
                        sb.AppendLine($"已下架");
                    }

                    if (steam.EvaluationCount > 0)
                    {
                        sb.AppendLine($"{steam.RecommendationRate}% 好评（{steam.EvaluationCount}条评测）");
                    }
                    if(plainText)
                    {
                        sb.AppendLine($"https://store.steampowered.com/app/{steamId}");
                    }
                    else
                    {
                        sb.AppendLine($"<a href=\"https://store.steampowered.com/app/{steamId}\">Steam商店页面</a>");

                    }

                    if (entry.EntryRelationFromEntryNavigation.Any(s => s.ToEntryNavigation.Type == EntryType.Role))
                    {
                        sb.AppendLine();
                        sb.Append($"登场角色：");
                        var roles = entry.EntryRelationFromEntryNavigation.Where(s => s.ToEntryNavigation.Type == EntryType.Role).Select(s => s.ToEntryNavigation).ToList();
                        foreach (var item in roles)
                        {
                            if (roles.IndexOf(item) != 0)
                            {
                                sb.Append(",");
                            }
                            sb.Append($"<a href=\"https://www.cngal.org/entries/index/{item.Id}\">{item.DisplayName}</a>");
                        }
                    }
                }
            }



            foreach (var item in model.AddInfors)
            {

                sb.Append($"{item.Modifier}：");
                foreach (var temp in item.Contents)
                {
                    if (item.Contents.IndexOf(temp) != 0)
                    {
                        sb.Append(",");
                    }
                    sb.Append($"<a href=\"https://www.cngal.org/entries/index/{temp.Id}\">{temp.DisplayName}</a>");
                }
                sb.AppendLine();
            }

            return sb.ToString();

        }

        public async Task<string> GetNewestEditGames()
        {
            var tempDateTimeNow = DateTime.Now.ToCstTime();

            var entryIds = await _entryRepository.GetAll().AsNoTracking().OrderByDescending(s => s.PubulishTime)
                    .Where(s => s.Type == EntryType.Game && s.PubulishTime < tempDateTimeNow && string.IsNullOrWhiteSpace(s.Name) == false && s.IsHidden != true).Select(s => s.Id).Take(6).ToListAsync();
            entryIds.AddRange(await _entryRepository.GetAll().AsNoTracking()
                    .Where(s => s.Type == EntryType.Game && string.IsNullOrWhiteSpace(s.Name) == false && s.IsHidden != true && s.PubulishTime > tempDateTimeNow)
                    .OrderBy(s => s.PubulishTime).Select(s => s.Id).Take(6).ToListAsync());


            //获取近期编辑
            var games = await _entryRepository.GetAll().OrderByDescending(s => s.LastEditTime).AsNoTracking()
                .Where(s => s.Type == EntryType.Game && string.IsNullOrWhiteSpace(s.Name) == false && s.IsHidden != true && entryIds.Contains(s.Id) == false).Take(6).ToListAsync();

            StringBuilder sb = new StringBuilder();

            foreach (var item in games)
            {
                sb.Append($"<a href=\"https://www.cngal.org/entries/index/{item.Id}\">《{item.DisplayName}》</a>");

                sb.Append($" - {item.LastEditTime.ToTimeFromNowString()}");

                sb.AppendLine();
            }

            return sb.ToString();
        }

        public async Task<string> GetNewestNews()
        {
            var news = await _articleRepository.GetAll().Include(s => s.CreateUser)
              .Include(s => s.CreateUser)
              .Include(s => s.Entries)
              .OrderByDescending(s => s.RealNewsTime).ThenByDescending(s => s.PubishTime)
              .Where(s => s.IsHidden != true && s.Type == ArticleType.News && string.IsNullOrWhiteSpace(s.Name) == false).AsNoTracking().Take(6).ToListAsync();

            StringBuilder sb = new StringBuilder();
            foreach (var item in news)
            {
                var infor = await _articleService.GetNewsModelAsync(item);
                sb.Append($"【{infor.GroupName}】");

                sb.Append($"<a href=\"{(string.IsNullOrWhiteSpace(infor.Link) ? ("https://www.cngal.org/articles/index/" + item.Id) : infor.Link)}\">《{item.DisplayName}》</a>");

                sb.Append($" - {infor.HappenedTime.ToTimeFromNowString()}");

                sb.AppendLine();
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public async Task<string> GetNewestPublishGames()
        {
            try
            {
                var dateTime = DateTime.Now.ToCstTime();
                //获取即将发售
                var games = await _entryRepository.GetAll().AsNoTracking()
                    .Include(s=>s.Information)
                    .Where(s => s.Type == EntryType.Game && string.IsNullOrWhiteSpace(s.Name) == false && s.IsHidden != true && s.PubulishTime <= dateTime)
                    .OrderByDescending(s => s.PubulishTime).Take(6).ToListAsync();

                StringBuilder sb = new StringBuilder();
                foreach (var item in games)
                {
                    sb.Append($"【{item.PubulishTime?.ToString("M月d日")}】");
                    sb.Append($"<a href=\"https://www.cngal.org/entries/index/{item.Id}\">《{item.DisplayName}》</a>");

                    sb.AppendLine();
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public async Task<string> GetNewestUnPublishGames()
        {
            try
            {
                var dateTime = DateTime.Now.ToCstTime();
                //获取即将发售
                var games = await _entryRepository.GetAll().AsNoTracking()
                    .Include(s => s.Information)
                    .Where(s => s.Type == EntryType.Game && string.IsNullOrWhiteSpace(s.Name) == false && s.IsHidden != true && s.PubulishTime > dateTime)
                    .OrderBy(s => s.PubulishTime).Take(6).ToListAsync();

                StringBuilder sb = new StringBuilder();
                foreach (var item in games)
                {
                    sb.Append($"【{item.PubulishTime?.ToString("M月d日")}】");

                    sb.Append($"<a href=\"https://www.cngal.org/entries/index/{item.Id}\">《{item.DisplayName}》</a>");

                    sb.AppendLine();
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}