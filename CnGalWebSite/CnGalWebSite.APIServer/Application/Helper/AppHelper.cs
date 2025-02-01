using CnGalWebSite.APIServer.Application.Users;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Helper;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.Models;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Others;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.Helper.Extensions;
using Gt3_server_csharp_aspnetcoremvc_bypass.Controllers.Sdk;
using IdentityModel;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using Operation = CnGalWebSite.DataModel.Model.Operation;
using Microsoft.AspNetCore.Authentication;
using CnGalWebSite.Core.Models;
using HtmlAgilityPack;
using NuGet.Versioning;

namespace CnGalWebSite.APIServer.Application.Helper
{
    public class AppHelper : IAppHelper
    {


        private readonly IRepository<Entry, int> _entryRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IRepository<Examine, long> _examineRepository;
        private readonly IRepository<Article, long> _articleRepository;
        private readonly IRepository<Tag, int> _tagRepository;
        private readonly IRepository<TokenCustom, int> _tokenCustomRepository;
        private readonly IRepository<FileManager, int> _fileManagerRepository;
        private readonly IRepository<Comment, long> _commentRepository;
        private readonly IRepository<ApplicationUser, string> _userRepository;
        private readonly IRepository<HistoryUser, int> _historyUserRepository;
        private readonly IRepository<UserFile, int> _userFileRepository;
        private readonly IRepository<BackUpArchive, long> _backUpArchiveRepository;
        private readonly IRepository<BackUpArchiveDetail, long> _backUpArchiveDetailRepository;
        private readonly IRepository<SignInDay, long> _signInDayRepository;
        private readonly IRepository<FavoriteFolder, long> _favoriteFolderRepository;
        private readonly IRepository<SendCount, long> _sendCountRepository;
        private readonly IRepository<Loginkey, long> _loginkeyRepository;
        private readonly IRepository<PlayedGame, long> _playedGameRepository;

        private readonly IRepository<UserIntegral, long> _userIntegralRepository;
        private readonly IEmailService _EmailService;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public AppHelper(IRepository<BackUpArchive, long> backUpArchiveRepository, IRepository<SignInDay, long> signInDayRepository, IRepository<BackUpArchiveDetail, long> backUpArchiveDetailRepository, IRepository<UserFile, int> userFileRepository,
            IRepository<FavoriteFolder, long> favoriteFolderRepository, IRepository<HistoryUser, int> historyUserRepository, IRepository<ApplicationUser, string> userRepository,
            IRepository<Comment, long> commentRepository, IConfiguration configuration, IRepository<Loginkey, long> loginkeyRepository,
        IHttpClientFactory clientFactory, IRepository<FileManager, int> fileManagerRepository, IEmailService EmailService, IRepository<TokenCustom, int> tokenCustomRepository,
            IRepository<Article, long> aricleRepository, IRepository<Entry, int> entryRepository, IRepository<SendCount, long> sendCountRepository, HttpClient httpClient,
        IWebHostEnvironment webHostEnvironment, IRepository<Examine, long> examineRepository, IRepository<Tag, int> tagRepository, IRepository<PlayedGame, long> playedGameRepository,
        IRepository<UserIntegral, long> userIntegralRepository)
        {
            _entryRepository = entryRepository;
            _examineRepository = examineRepository;
            _tagRepository = tagRepository;
            _articleRepository = aricleRepository;
            _EmailService = EmailService;
            _webHostEnvironment = webHostEnvironment;
            _fileManagerRepository = fileManagerRepository;
            _tokenCustomRepository = tokenCustomRepository;
            _clientFactory = clientFactory;
            _configuration = configuration;
            _userRepository = userRepository;
            _commentRepository = commentRepository;
            _examineRepository = examineRepository;
            _historyUserRepository = historyUserRepository;
            _backUpArchiveDetailRepository = backUpArchiveDetailRepository;
            _backUpArchiveRepository = backUpArchiveRepository;
            _userFileRepository = userFileRepository;
            _signInDayRepository = signInDayRepository;
            _favoriteFolderRepository = favoriteFolderRepository;
            _sendCountRepository = sendCountRepository;
            _loginkeyRepository = loginkeyRepository;
            _userIntegralRepository = userIntegralRepository;
            _playedGameRepository = playedGameRepository;
            _httpClient = httpClient;
        }

        public async Task<bool> IsEntryLockedAsync(long entryId, string userId, Operation operation)
        {
            return await _examineRepository.FirstOrDefaultAsync(s => s.EntryId == entryId && s.ApplicationUserId != userId && s.IsPassed == null && s.Operation == operation) != null;
        }

        public string GetImagePath(string image, string defaultStr)
        {
            return ToolHelper.GetImagePath(image, defaultStr);
        }

        public string GetStringAbbreviation(string str, int length)
        {
            if (str == null)
            {
                return "";
            }
            return str.Length < length * 2.5 ? str : string.Concat(str.AsSpan(0, (int)(length * 2.5)), "......");
        }


        public async Task<ApplicationUser> GetAPICurrentUserAsync(HttpContext context)
        {
            var id = context.User?.Claims?.GetUserId();
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }
            var user = await _userRepository.FirstOrDefaultAsync(s => s.Id == id);
            if (user != null)
            {
                //是否更新SteamId
                if (string.IsNullOrWhiteSpace(user.SteamId) || user.SteamId.Replace("、", ",").Replace("，", ",").Contains(',') == false)
                {
                    var steamId = context.User?.Claims?.GetUserSteamId();
                    if (string.IsNullOrWhiteSpace(steamId) == false)
                    {
                        user.SteamId = context.User?.Claims?.GetUserSteamId();
                        await _userRepository.UpdateAsync(user);
                    }
                }
                return user;
            }

            //新用户
            var (email, name) = context.User.Claims.GetExternalUserInfor();
            //if (await _userRepository.AnyAsync(s => s.Email == email))
            //{
            //    email = null;
            //}
            while (await _userRepository.AnyAsync(s => s.UserName == name))
            {
                name = "用户_" + new Random(DateTime.Now.Millisecond).Next(0, 99999).ToString();
            }
            user = new ApplicationUser
            {
                Id = id,
                UserName = name,
                NormalizedUserName = name.ToUpper(),
                Email = email,
                NormalizedEmail = email.ToUpper(),
                RegistTime = DateTime.Now.ToCstTime(),
                LastOnlineTime = DateTime.Now.ToCstTime()
            };

            try
            {
                await _userRepository.InsertAsync(user);
            }
            catch
            {
                // 插入失败说明同时并发的请求已经插入了
            }
            
            return user;
        }

        public async Task<bool> DeleteFieAsync(ApplicationUser user, string fileName)
        {
            //获取文件管理
            var fileManager = await _fileManagerRepository.GetAll().Include(s => s.UserFiles).FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id);
            if (fileManager == null)
            {
                return false;
            }
            else
            {
                var userFile = fileManager.UserFiles.FirstOrDefault(s => s.FileName == fileName);
                if (userFile != null)
                {
                    _ = DeleteFile(fileName);
                    _ = fileManager.UserFiles.Remove(userFile);
                    fileManager.UsedSize -= (long)userFile.FileSize;
                    _ = _fileManagerRepository.Update(fileManager);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool DeleteFile(string name)
        {
            try
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                uploadsFolder = Path.Combine(uploadsFolder, name);
                File.Delete(uploadsFolder);
                return true;
            }
            catch
            {
                return false;
            }

        }

        public async Task<string> GetTagListStringAsync(Tag tag)
        {
            if (tag == null)
            {
                return null;
            }
            else
            {
                var result = tag.Name;
                //尝试向上索引
                while (tag.ParentCodeNavigation != null)
                {
                    tag = await _tagRepository.GetAll().AsNoTracking().Include(s => s.ParentCodeNavigation).FirstOrDefaultAsync(s => s.Name == tag.ParentCodeNavigation.Name);
                    if (tag != null)
                    {
                        result = tag.Name + "->" + result;
                    }
                    else
                    {
                        break;
                    }
                }
                return result;
            }
        }

        public async Task<bool> IsTagLockedAsync(int tagId, string userId, Operation operation)
        {
            return await _examineRepository.GetAll().AsNoTracking().AnyAsync(s => s.TagId == tagId && s.ApplicationUserId != userId && s.IsPassed == null && s.Operation == operation) != false;
        }

        public async Task ArticleReaderNumUpAsync(long articleId)
        {
            _articleRepository.Clear();
            _ = await _articleRepository.GetAll().Where(s => s.Id == articleId).ExecuteUpdateAsync(s => s.SetProperty(a => a.ReaderCount, b => b.ReaderCount + 1));

        }

        public async Task EntryReaderNumUpAsync(int entryId)
        {
            _entryRepository.Clear();
            _ = await _entryRepository.GetAll().Where(s => s.Id == entryId).ExecuteUpdateAsync(s => s.SetProperty(s => s.ReaderCount, b => b.ReaderCount + 1));
        }

        public async Task UpdateFavoritesCountAsync(long[] favoriteFolderIds)
        {
            foreach (var item in favoriteFolderIds)
            {
                var count = await _favoriteFolderRepository.GetAll().Include(s => s.FavoriteObjects).Where(s => s.Id == item).Select(s => s.FavoriteObjects.Count).FirstOrDefaultAsync();
                _ = await _favoriteFolderRepository.GetAll().Where(s => s.Id == item).ExecuteUpdateAsync(s => s.SetProperty(s => s.Count, b => count));
            }
        }

        public async Task DeleteComment(long id)
        {
            var comment = await _commentRepository.GetAll().Include(s => s.InverseParentCodeNavigation).FirstOrDefaultAsync(s => s.Id == id);
            if (comment == null)
            {
                return;
            }
            //删除关联审核
            await _examineRepository.DeleteAsync(s => s.CommentId == id);
            //遍历删除子评论
            foreach (var item in comment.InverseParentCodeNavigation)
            {
                await DeleteComment(item.Id);
            }
            await _commentRepository.DeleteAsync(comment);
            return;
        }

        public async Task<bool> IsExceedMaxSendCount(string mail, int limit, int maxMinutes)
        {
            var timeNow = DateTime.Now.ToCstTime();
            var errors = await _sendCountRepository.GetAll().CountAsync(s => s.Mail == mail && s.SendTime.AddMinutes(maxMinutes) > timeNow);
            return errors >= limit;
        }

        public async Task AddSendCount(string mail, SendType type)
        {
            _ = await _sendCountRepository.InsertAsync(new SendCount
            {
                SendTime = DateTime.Now.ToCstTime(),
                Type = type,
                Mail = mail
            });
        }

        public bool CheckRecaptcha(string challenge, string validate, string seccode)
        {
            try
            {
                GeetestLibResult result = null;
                IDictionary<string, string> paramDict = new Dictionary<string, string> { };
                var gtLib = new GeetestLib(_configuration["GEETEST_ID"], _configuration["GEETEST_KEY"]);
                result = gtLib.SuccessValidate(challenge, validate, seccode, paramDict);
                return result.GetStatus() == 1;
            }
            catch
            {
                return false;
            }
        }

        public bool CheckRecaptcha(HumanMachineVerificationResult model)
        {
            return CheckRecaptcha(model.Challenge, model.Validate, model.Seccode);
        }



        public ArticleInforTipViewModel GetArticleInforTipViewModel(Article item)
        {
            return new ArticleInforTipViewModel
            {
                Id = item.Id,
                Name = item.Name,
                Type = item.Type,
                DisplayName = string.IsNullOrWhiteSpace(item.DisplayName) ? item.Name : item.DisplayName,
                CreateUserName = item.CreateUser?.UserName,
                CreateUserId = item.CreateUserId,
                MainImage = GetImagePath(item.MainPicture, "certificate.png"),
                BriefIntroduction = item.BriefIntroduction,
                LastEditTime = item.LastEditTime,
                ReaderCount = item.ReaderCount,
                ThumbsUpCount = item.ThumbsUpCount,
                CommentCount = item.CommentCount,
                Link = item.OriginalLink

            };
        }

        public VideoInforTipViewModel GetVideoInforTipViewModel(Video item)
        {
            return new VideoInforTipViewModel
            {
                Id = item.Id,
                Name = item.Name,
                Type = item.Type ?? "视频",
                DisplayName = item.DisplayName,
                CreateUserName = item.CreateUser?.UserName,
                CreateUserId = item.CreateUserId,
                MainImage = GetImagePath(item.MainPicture, "app.png"),
                BriefIntroduction = item.BriefIntroduction,
                LastEditTime = item.LastEditTime,
                ReaderCount = item.ReaderCount,
                CommentCount = item.CommentCount,
                PubishTime = item.PubishTime,
            };
        }

        public EntryInforTipViewModel GetEntryInforTipViewModel(Entry entry)
        {

            var model = new EntryInforTipViewModel
            {
                Id = entry.Id,
                Name = string.IsNullOrWhiteSpace(entry.DisplayName) ? entry.Name : entry.DisplayName,
                Type = entry.Type,
                BriefIntroduction = entry.BriefIntroduction,
                LastEditTime = entry.LastEditTime,
                ReaderCount = entry.ReaderCount,
                CommentCount = entry.CommentCount,
                PublishTime = entry.PubulishTime,
                AddInfors = new List<EntryInforTipAddInforModel>()
            };
            //预处理图片
            model.MainImage = entry.Type is EntryType.Staff or EntryType.Role
                ? GetImagePath(entry.Thumbnail, "user.png")
                : GetImagePath(entry.MainPicture, "app.png");

            //处理音频
            if (entry.Audio != null && entry.Audio.Any())
            {
                model.Audio.AddRange(entry.Audio.Select(s => new EditAudioAloneModel
                {
                    BriefIntroduction = s.BriefIntroduction,
                    Duration = s.Duration,
                    Name = s.Name,
                    Priority = s.Priority,
                    Thumbnail = s.Thumbnail,
                    Url = s.Url,
                }).OrderByDescending(s => s.Priority));
            }

            //处理附加信息
            if (entry.EntryRelationFromEntryNavigation != null && entry.EntryStaffFromEntryNavigation != null)
            {
                if (entry.Type == EntryType.Role)
                {
                    //查找配音
                    var cvs = entry.EntryStaffFromEntryNavigation.Where(s => s.PositionGeneral == PositionGeneralType.CV);
                    if (cvs.Any())
                    {
                        model.AddInfors.Add(new EntryInforTipAddInforModel
                        {
                            Modifier = "配音",
                            Contents = cvs.Select(s => new StaffNameModel
                            {
                                DisplayName = string.IsNullOrWhiteSpace(s.CustomName) ? (s.ToEntryNavigation?.Name ?? s.Name) : s.CustomName,
                                Id = s.ToEntryNavigation?.Id ?? -1
                            }).ToList()
                        });
                    }

                    //查找登场游戏
                    var gameNames = new List<StaffNameModel>();
                    foreach (var nav in entry.EntryRelationFromEntryNavigation.Where(s => s.ToEntryNavigation.IsHidden == false))
                    {
                        var item = nav.ToEntryNavigation;
                        if (item.Type == EntryType.Game && string.IsNullOrWhiteSpace(item.DisplayName) == false)
                        {
                            gameNames.Add(new StaffNameModel
                            {

                                DisplayName = item.DisplayName,
                                Id = item.Id
                            });
                            if (gameNames.Count >= 3)
                            {
                                break;
                            }
                        }
                    }
                    if (gameNames.Count > 0)
                    {
                        model.AddInfors.Add(new EntryInforTipAddInforModel
                        {
                            Modifier = "登场游戏",
                            Contents = gameNames
                        });
                    }
                }
                else if (entry.Type == EntryType.Staff)
                {
                    //查找参与作品
                    var gameNames = entry.EntryStaffToEntryNavigation
                        .Where(s => s.PositionGeneral != PositionGeneralType.SpecialThanks && s.ToEntry == entry.Id)
                        .Where(s => s.FromEntryNavigation != null && s.FromEntryNavigation.IsHidden == false && s.FromEntryNavigation.Type == EntryType.Game && string.IsNullOrWhiteSpace(s.FromEntryNavigation.Name) == false)
                        .GroupBy(s => s.FromEntry)
                        .Take(3)
                        .Select(s => new StaffNameModel
                        {
                            DisplayName = s.First().FromEntryNavigation.DisplayName,
                            Id = s.First().FromEntryNavigation.Id
                        }).ToList();
                    if (gameNames.Any())
                    {
                        model.AddInfors.Add(new EntryInforTipAddInforModel
                        {
                            Modifier = "参与作品",
                            Contents = gameNames
                        });
                    }
                }
                else if (entry.Type == EntryType.ProductionGroup)
                {
                    //查找参与作品
                    var gameNames = entry.EntryStaffToEntryNavigation
                        .Where(s => s.PositionGeneral != PositionGeneralType.SpecialThanks && s.ToEntry == entry.Id)
                        .Where(s => s.FromEntryNavigation != null && s.FromEntryNavigation.IsHidden == false && s.FromEntryNavigation.Type == EntryType.Game && string.IsNullOrWhiteSpace(s.FromEntryNavigation.Name) == false)
                        .GroupBy(s => s.FromEntry)
                        .Take(3)
                        .Select(s => new StaffNameModel
                        {
                            DisplayName = s.First().FromEntryNavigation.DisplayName,
                            Id = s.First().FromEntryNavigation.Id
                        }).ToList();
                    if (gameNames.Any())
                    {
                        model.AddInfors.Add(new EntryInforTipAddInforModel
                        {
                            Modifier = "参与作品",
                            Contents = gameNames
                        });
                    }
                }

            }

            return model;
        }

        public TagInforTipViewModel GetTagInforTipViewModel(Tag item)
        {
            return new TagInforTipViewModel
            {
                Id = item.Id,
                Name = item.Name,
                MainImage = GetImagePath(item.MainPicture, "app.png"),
                BriefIntroduction = item.BriefIntroduction,
                LastEditTime = item.LastEditTime,
                ReaderCount = item.ReaderCount
            };
        }

        public PeripheryInforTipViewModel GetPeripheryInforTipViewModel(Periphery periphery)
        {
            //预处理图片
            periphery.MainPicture = GetImagePath(periphery.MainPicture, "app.png");


            periphery.BriefIntroduction = GetStringAbbreviation(periphery.BriefIntroduction, 50);

            var model = new PeripheryInforTipViewModel
            {
                Id = periphery.Id,
                Name = periphery.DisplayName ?? periphery.Name,
                MainImage = periphery.MainPicture,
                BriefIntroduction = periphery.BriefIntroduction,
                LastEditTime = periphery.LastEditTime,
                ReaderCount = periphery.ReaderCount,
                CommentCount = periphery.CommentCount,
                AddInfors = new List<EntryInforTipAddInforModel>(),
                Type = periphery.Type,
                Category = periphery.Category
            };

            //处理附加信息
            if (periphery.RelatedEntries != null && periphery.RelatedEntries.Any())
            {
                var temp = new EntryInforTipAddInforModel
                {
                    Modifier = "关联词条",
                    Contents = new List<StaffNameModel>()
                };
                model.AddInfors.Add(temp); ;
                foreach (var item in periphery.RelatedEntries)
                {
                    temp.Contents.Add(new StaffNameModel
                    {
                        DisplayName = item.DisplayName,
                        Id = item.Id,
                    });


                }
            }

            return model;
        }


        private static string ConvertJsonString(string str)
        {
            //格式化json字符串
            var serializer = new Newtonsoft.Json.JsonSerializer();
            TextReader tr = new StringReader(str);
            var jtr = new JsonTextReader(tr);
            var obj = serializer.Deserialize(jtr);
            if (obj != null)
            {
                var textWriter = new StringWriter();
                var jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            else
            {
                return str;
            }
        }

        public string GetJsonStringView(string jsonStr)
        {
            if (jsonStr == null)
            {
                return "";
            }
            var resulte = "``` json\n" + ConvertJsonString(jsonStr) + "\n```";
            //使用markdown渲染
            return MarkdownToHtml(resulte);
        }

        #region Markdown 转 Html

        public string MarkdownToHtml(string text)
        {
            if (text == null)
            {
                return "";
            }

            var sb = new StringBuilder(text);

            //去除代码块
            text = RemoveCode(text);

            //为图片添加注释
            sb = ProcImageText(sb, text);


            //转换Bilibili视频

            sb = ProcBilibiliVideo(sb, text);



            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseSoftlineBreakAsHardlineBreak().UseFigures().Build();
            var html = Markdown.ToHtml(sb.ToString(), pipeline);
            html = AddLazyLoading(html);
            return html;
        }

        private string AddLazyLoading(string html)
        {
            return html.Replace("<img", "<img loading=\"lazy\" ");
        }

        private string RemoveCode(string text)
        {
            while (true)
            {
                var mid = text.MidStrEx("```", "```");
                if (string.IsNullOrWhiteSpace(mid))
                {
                    break;
                }

                text = text.Replace($"```{mid}```", "");
            }

            return text;
        }

        private StringBuilder ProcImageText(StringBuilder sb, string text)
        {
            //查找所有图片链接
            var regImg = new Regex(@"\!\[.*?]\(.*?\)", RegexOptions.IgnoreCase);

            var images = regImg.Matches(text).Where(s => string.IsNullOrWhiteSpace(s.Value) == false).Select(s => s.Value).ToList().Purge();

            foreach (var item in images)
            {
                //找到注释
                var infor = new Regex(@"(?<=\!\[)(.+?)(?=\]\(.*?\))", RegexOptions.IgnoreCase).Match(item).Value;

                if (string.IsNullOrWhiteSpace(infor) || infor.ToLower() == "image")
                {
                    continue;
                }

                sb = sb.Replace(item, $"{item}**{infor.Trim()}**\n");
            }

            return sb;
        }

        private StringBuilder ProcBilibiliVideo(StringBuilder sb, string text)
        {
            var regLink = new Regex(@"\[.*?]\(.*?\)", RegexOptions.IgnoreCase);

            var links = regLink.Matches(text).Select(s => s.Value);
            links = links.Where(s => string.IsNullOrWhiteSpace(s) == false && s.Contains("https://www.bilibili.com/video")).ToList().Purge();

            foreach (var item in links)
            {
                var id = item.MidStrEx("https://www.bilibili.com/video/", "/") ?? item.MidStrEx("https://www.bilibili.com/video/", "?") ?? item.MidStrEx("https://www.bilibili.com/video/", ")");
                //找到注释
                var infor = new Regex(@"(?<=\[)(.+?)(?=\]\(.*?\))", RegexOptions.IgnoreCase).Match(item).Value;

                if (string.IsNullOrWhiteSpace(id) || (id[0] != 'B' && id[0] != 'a'))
                {
                    continue;
                }


                sb = sb.Replace(item, $"<div class=\"aspect-ratio\"><iframe src=\"https://player.bilibili.com/player.html?{(id[0] == 'B' ? $"bvid={id}" : id[0] == 'a' ? $"aid={id[2..^1]}" : "")}&high_quality=1&autoplay=0\" scrolling=\"no\" border=\"0\" frameborder=\"no\" framespacing=\"0\" allowfullscreen=\"true\" width=\"100%\" height=\"500px\"></iframe></div>\n\n{(string.IsNullOrWhiteSpace(infor) ? "" : $"**{infor.Trim()}**\n\n")}");

            }

            return sb;
        }

        #endregion


        public async Task<long> GetUserIntegral(string userId, DateTime time)
        {
            //计算积分
            //编辑 词条  标签  消歧义页  评论  发表文章
            //积分  10    8      2         5     50
            //
            var signInDaysIntegral = 5;
            var entryIntegral = 50;

            var tagIntegral = signInDaysIntegral * 6;
            var disambigIntegral = signInDaysIntegral * 6;
            var peripheryIntegral = entryIntegral;
            var commentIntegral = signInDaysIntegral;
            var commentLimited = 5;
            var articleIntegral = entryIntegral;

            try
            {
                //编辑
                var temp_1 = await _examineRepository.GetAll().Where(s => s.ApplyTime > time).Where(s => s.ApplicationUserId == userId && s.IsPassed == true)
                 .Select(n => new { Time = n.ApplyTime.Date, n.EntryId, n.TagId, n.ArticleId, n.CommentId, n.DisambigId, n.PeripheryId })
                 // 分类
                 .ToListAsync();
                // 返回汇总样式
                var temp_2 = temp_1.GroupBy(s => s.Time);
                var integral_1 = (temp_2.Select(s => s.Where(s => s.EntryId != null).GroupBy(s => s.EntryId).Count()).Sum() * entryIntegral)
                                  + (temp_2.Select(s => s.Where(s => s.TagId != null).GroupBy(s => s.TagId).Count()).Sum() * tagIntegral)
                                  + (temp_2.Select(s => s.Where(s => s.DisambigId != null).GroupBy(s => s.DisambigId).Count()).Sum() * disambigIntegral)
                                  + (temp_2.Select(s => s.Where(s => s.PeripheryId != null).GroupBy(s => s.PeripheryId).Count()).Sum() * peripheryIntegral)
                                  + (temp_2.Select(s => s.Where(s => s.CommentId != null).GroupBy(s => s.DisambigId)
                                            .Select(s => s.Count() > commentLimited ? commentLimited : s.Count()).Sum()).Sum() * commentLimited);


                //发表文章
                var integral_2 = await _articleRepository.GetAll().Where(s => s.CreateTime > time).CountAsync(s => s.CreateUserId == userId) * articleIntegral;

                return integral_1 + integral_2;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public async Task GetAllSteamImageToGame()
        {
            var entries = await _entryRepository.GetAll().Include(s => s.Information).Where(s => string.IsNullOrWhiteSpace(s.MainPicture) && s.Type == EntryType.Game).ToListAsync();

            foreach (var item in entries)
            {
                //查找steamId
                string steamId = null;
                foreach (var infor in item.Information)
                {
                    if (infor.DisplayName == "Steam平台Id")
                    {
                        if (string.IsNullOrWhiteSpace(infor.DisplayValue) == false)
                        {
                            steamId = infor.DisplayValue;
                            break;
                        }

                    }
                }

                if (string.IsNullOrWhiteSpace(steamId) == false)
                {
                    item.MainPicture = "https://media.st.dl.eccdnx.com/steam/apps/" + steamId + "/header.jpg";
                    _ = await _entryRepository.UpdateAsync(item);
                }
            }
        }

        public async Task ChangeAllImagesPath()
        {
            var oldName = "pic.sliots.top";
            var newName = "pic.cngal.top";

            //查找词条
            var entries = await _entryRepository.GetAll().Include(s => s.Pictures).ToListAsync();
            foreach (var item in entries)
            {

                if (string.IsNullOrWhiteSpace(item.SmallBackgroundPicture) == false)
                {
                    item.SmallBackgroundPicture = item.SmallBackgroundPicture.Replace(oldName, newName);
                }
                if (string.IsNullOrWhiteSpace(item.BackgroundPicture) == false)
                {
                    item.BackgroundPicture = item.BackgroundPicture.Replace(oldName, newName);
                }
                if (string.IsNullOrWhiteSpace(item.MainPage) == false)
                {
                    item.MainPage = item.MainPage.Replace(oldName, newName);
                }

                foreach (var infor in item.Pictures)
                {
                    if (string.IsNullOrWhiteSpace(infor.Url) == false)
                    {
                        infor.Url = infor.Url.Replace(oldName, newName);
                    }
                }

                _ = await _entryRepository.UpdateAsync(item);
            }
            //查找文章
            var articles = await _articleRepository.GetAll().ToListAsync();
            foreach (var item in articles)
            {

                if (string.IsNullOrWhiteSpace(item.SmallBackgroundPicture) == false)
                {
                    item.SmallBackgroundPicture = item.SmallBackgroundPicture.Replace(oldName, newName);
                }
                if (string.IsNullOrWhiteSpace(item.BackgroundPicture) == false)
                {
                    item.BackgroundPicture = item.BackgroundPicture.Replace(oldName, newName);
                }
                if (string.IsNullOrWhiteSpace(item.MainPage) == false)
                {
                    item.MainPage = item.MainPage.Replace(oldName, newName);
                }

                _ = await _articleRepository.UpdateAsync(item);
            }
            //查找用户
            var users = await _userRepository.GetAll().ToListAsync();
            foreach (var item in users)
            {

                if (string.IsNullOrWhiteSpace(item.SBgImage) == false)
                {
                    item.SBgImage = item.SBgImage.Replace(oldName, newName);
                }
                if (string.IsNullOrWhiteSpace(item.MBgImage) == false)
                {
                    item.MBgImage = item.MBgImage.Replace(oldName, newName);
                }
                if (string.IsNullOrWhiteSpace(item.BackgroundImage) == false)
                {
                    item.BackgroundImage = item.BackgroundImage.Replace(oldName, newName);
                }
                if (string.IsNullOrWhiteSpace(item.MainPageContext) == false)
                {
                    item.MainPageContext = item.MainPageContext.Replace(oldName, newName);
                }

                _ = await _userRepository.UpdateAsync(item);
            }
            //查找所有文件
            var files = await _userFileRepository.GetAll().ToListAsync();
            foreach (var item in files)
            {

                if (string.IsNullOrWhiteSpace(item.FileName) == false)
                {
                    item.FileName = item.FileName.Replace(oldName, newName);
                }


                _ = await _userFileRepository.UpdateAsync(item);
            }
        }

        public async Task<string> SetUserLoginKeyAsync(string userId, bool isOneTime = false)
        {
            var tempDateTimeNow = DateTime.Now.ToCstTime();
            var keyStr = Guid.NewGuid().ToString();
            var key = await _loginkeyRepository.FirstOrDefaultAsync(s => s.UserId == userId);
            if (key == null)
            {
                _ = await _loginkeyRepository.InsertAsync(new Loginkey
                {
                    UserId = userId,
                    Key = keyStr,
                    IsOneTime = isOneTime,
                    CreateTime = tempDateTimeNow,
                });
            }
            else
            {
                key.CreateTime = tempDateTimeNow;
                key.Key = keyStr;
                key.IsOneTime = isOneTime;
                _ = await _loginkeyRepository.UpdateAsync(key);
            }
            return keyStr;
        }

        public async Task<string> GetUserFromLoginKeyAsync(string key)
        {
            var tempDateTimeNow = DateTime.Now.ToCstTime();
            var result = await _loginkeyRepository.FirstOrDefaultAsync(s => s.Key == key && s.CreateTime.AddHours(1) > tempDateTimeNow);
            if (result != null)
            {
                if (result.IsOneTime)
                {
                    await _loginkeyRepository.DeleteAsync(result);
                }
                return result.UserId;
            }
            return null;
        }

        public async Task DeleteAllBackupInfor()
        {
            await _backUpArchiveDetailRepository.GetAll().Where(s => true).ExecuteDeleteAsync();
            await _backUpArchiveRepository.GetAll().Where(s => true).ExecuteDeleteAsync();
        }
    }
}
