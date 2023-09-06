using CnGalWebSite.APIServer.Application.Articles;
using CnGalWebSite.APIServer.Application.Comments;
using CnGalWebSite.APIServer.Application.Entries;
using CnGalWebSite.APIServer.Application.Favorites;
using CnGalWebSite.APIServer.Application.Files;
using CnGalWebSite.APIServer.Application.GPT;
using CnGalWebSite.APIServer.Application.Helper;
using CnGalWebSite.APIServer.Application.Lotteries;
using CnGalWebSite.APIServer.Application.Messages;
using CnGalWebSite.APIServer.Application.Peripheries;
using CnGalWebSite.APIServer.Application.Ranks;
using CnGalWebSite.APIServer.Application.Search;
using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.Application.Votes;
using CnGalWebSite.APIServer.Application.WeiXin;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.APIServer.ExamineX;
using CnGalWebSite.DataModel.Application.Search.Dtos;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.ImportModel;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.Models;
using CnGalWebSite.DataModel.ViewModel.Robots;
using CnGalWebSite.Helper.Extensions;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Org.BouncyCastle.Utilities;
using Senparc.Weixin.MP.AdvancedAPIs.MerChant;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace CnGalWebSite.APIServer.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/robot/[action]")]
    public class RobotAPIController : ControllerBase
    {
        
        
        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IRepository<StoreInfo, long> _storeInfoRepository;
        private readonly IUserService _userService;
        private readonly IWeiXinService _weiXinService;
        private readonly IChatGPTService _chatGPTService;

        public RobotAPIController(IRepository<StoreInfo, long> storeInfoRepository,IWeiXinService weiXinService,
         IUserService userService,  IChatGPTService chatGPTService,
       IRepository<Entry, int> entryRepository)
        {
            _storeInfoRepository = storeInfoRepository;
            _entryRepository = entryRepository;
            _userService = userService;
            _weiXinService = weiXinService;
            _chatGPTService= chatGPTService;
        }

       
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<Result>> GetArgValueAsync(GetArgValueModel model)
        {
            if (model.Name == "auth")
            {
                //var user = await _userRepository.GetAll().AsNoTracking()
                //    .FirstOrDefaultAsync(s => s.GroupQQ == model.SenderId);

                //if (user == null)
                //{
                //    return new Result { Successful = false, Error = "你还没有绑定账号哦~ \nPS：私聊“绑定账号”试试吧ヾ(•ω•`)o" };
                //}

                //return new Result { Successful = true, Error = (await _userManager.GetRolesAsync(user)).FirstOrDefault() };

                return new Result { Successful = false, Error = "看板娘好像忘记了什么，到底是什么呢？" };
            }
            else if (model.Name == "bindqq")
            {
                var temps = model.Infor.Split("绑定");
                if (temps.Length <= 1)
                {
                    return new Result { Successful = true, Error = "" };
                }
                var code = temps[1].Replace("绑定", "").Trim();


                if ((await _userService.BindGroupQQ(code, model.SenderId)).Successful)
                {
                    return new Result { Successful = true, Error = "o(〃＾▽＾〃)o 成功绑定账号" };
                }
                else
                {
                    return new Result { Successful = false, Error = "＞﹏＜ 看板娘觉得身份识别码错了喵~" };
                }
            }
            else if (model.Name == "introduce")
            {
                var entryName = model.Infor.Trim();

                if(string.IsNullOrWhiteSpace(entryName))
                {
                    return new Result { Successful = false, Error = "呜呜呜~~~ 找不到这个词条" };
                }

                var entry = await _entryRepository.GetAll().AsNoTracking()
                    .Where(s => s.IsHidden == false && string.IsNullOrWhiteSpace(s.DisplayName) == false)
                    .Where(s => entryName.Length < 2 ? (s.DisplayName == entryName || s.AnotherName == entryName) : (s.DisplayName.Contains(entryName) || (s.AnotherName != null && s.AnotherName.Contains(entryName))))
                    .Select(s => new { s.Id, s.DisplayName })
                    .FirstOrDefaultAsync();

                if (entry == null)
                {
                    return new Result { Successful = false, Error = $"呜呜呜~~~ 找不到这个词条，你可以亲自创建这个词条哦~\nhttps://app.cngal.org/entries/establishentry?Name={UrlEncoder.Default.Encode(entryName)}" };
                }
                else
                {
                    if (entry.DisplayName != entryName && entry.DisplayName != entryName)
                    {
                        return new Result { Successful = true, Error = (await _weiXinService.GetEntryInfor(entry.Id, true, true,model.SenderId!=0)).DeleteHtmlLinks() + "\n（不太确定是不是这个词条哦~" };
                    }
                    else
                    {
                        return new Result { Successful = true, Error = (await _weiXinService.GetEntryInfor(entry.Id, true, true, model.SenderId != 0)).DeleteHtmlLinks() };
                    }
                }
            }
            else if (model.Name == "website")
            {
                var urls = Regex.Matches(model.Infor, "http[s]?://(?:(?!http[s]?://)[a-zA-Z]|[0-9]|[$\\-_@.&+/]|[!*\\(\\),]|(?:%[0-9a-fA-F][0-9a-fA-F]))+");

                //处理链接
                foreach (var item in urls.Select(s => s.ToString().Trim()))
                {
                    if (item.Contains("entries"))
                    {
                        var idStr = item.Split('/').Last();
                        if (int.TryParse(idStr, out var id))
                        {
                            return new Result { Successful = true, Error = (await _weiXinService.GetEntryInfor(id, true)).DeleteHtmlLinks() };
                        }
                    }
                    else if (item.Contains("articles"))
                    {
                        var idStr = item.Split('/').Last();
                        if (int.TryParse(idStr, out var id))
                        {
                            return new Result { Successful = true, Error = (await _weiXinService.GetArticleInfor(id, true)).DeleteHtmlLinks() };
                        }
                    }
                }

                return new Result { Successful = false, Error = null };

            }
            else if (model.Name == "steamdiscount")
            {
                var count = await _storeInfoRepository.GetAll().Include(s => s.Entry).CountAsync(s =>s.PlatformType== PublishPlatformType.Steam&& s.CutNow > 0 && s.Entry.IsHidden == false && string.IsNullOrWhiteSpace(s.Entry.Name) == false);

                return new Result { Successful = true, Error = $"今天有{count}款作品打折中：https://www.cngal.org/discount" };
            }
            else
            {
                var value = model.Name switch
                {
                    "recommend" => await _weiXinService.GetRandom(true, true),
                    "birthday" => await _weiXinService.GetRoleBirthdays( true)??"",
                    "BirthdayWithDefault" => await _weiXinService.GetRoleBirthdays(true)??"好像今天没人过生日~~~",
                    "NewestEditGames" => await _weiXinService.GetNewestEditGames(true),
                    "NewestUnPublishGames" => await _weiXinService.GetNewestUnPublishGames(true),
                    "NewestPublishGames" => await _weiXinService.GetNewestPublishGames(true),
                    "chatgpt" => await _chatGPTService.GetReply(model.Infor),
                    "NewestNews" => await _weiXinService.GetNewestNews(true, model.SenderId != 0),
                    _ => ""
                };

                return new Result { Successful = true, Error = value?.DeleteHtmlLinks() };
            }

        }
    }
}
