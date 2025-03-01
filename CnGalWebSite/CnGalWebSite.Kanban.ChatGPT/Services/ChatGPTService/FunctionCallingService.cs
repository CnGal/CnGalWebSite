using CnGalWebSite.Core.Services;
using CnGalWebSite.DataModel.Application.Search.Dtos;
using CnGalWebSite.DataModel.ExamineModel.Entries;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.Home;
using CnGalWebSite.Kanban.ChatGPT.Models.Functions;
using CnGalWebSite.Kanban.ChatGPT.Models.GPT;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using System;
using System.Text.Json;

namespace CnGalWebSite.Kanban.ChatGPT.Services.ChatGPTService
{
    public interface IFunctionCallingService
    {
        Task<string> ExecuteFunction(string functionName, string arguments);
        List<ChatCompletionTool> GetAvailableTools();
    }

    public class FunctionCallingService : IFunctionCallingService
    {
        private readonly Dictionary<string, Func<string, Task<string>>> _functionMap;
        private ILogger<FunctionCallingService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpService _httpService;

        private static int ExtractIdFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return 0;
            }

            string[] parts = url.Split('/');
            if (parts.Length > 0 && int.TryParse(parts[^1], out int id))
            {
                return id;
            }

            return 0;
        }

        public static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private readonly string _apiUrl;

        public FunctionCallingService(ILogger<FunctionCallingService> logger, IConfiguration configuration, IHttpService httpService)
        {
            _logger = logger;
            _configuration = configuration;
            _httpService = httpService;

            _functionMap = new Dictionary<string, Func<string, Task<string>>>
            {
                { "search_cngal", SearchCnGal },
                { "get_latest_games", GetLatestGames },
                { "get_latest_demo_games", GetLatestDemoGames },
                { "get_upcoming_games", GetUpcomingGames },
                { "get_news", GetNews },
                { "get_entry", GetEntry },
                { "get_article", GetArticle },
            };
            _apiUrl = _configuration["WebApiPath"] ?? "https://api.cngal.org/";
        }

        public List<ChatCompletionTool> GetAvailableTools()
        {
            return
            [
                new ChatCompletionTool
                {
                    Function = new ChatCompletionFunction
                    {
                        Name = "search_cngal",
                        Description = "如果遇到不懂的信息，优先在CnGal资料站中进行搜索。减少使用限定词，直接搜索条目名称。返回值中有条目的Id，通过Id可以获取条目详情，以及获取条目关联信息。",
                        Parameters = new ChatCompletionParameters
                        {
                            Properties = new Dictionary<string, ChatCompletionProperty>
                            {
                                ["text"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "搜索的文本"
                                },
                                ["current_page"] = new ChatCompletionProperty
                                {
                                    Type = "integer",
                                    Description = "当前的页数，默认值：1"
                                }
                            },
                            Required = ["text"]
                        }
                    }
                },
                new ChatCompletionTool
                {
                    Function = new ChatCompletionFunction
                    {
                        Name = "get_entry",
                        Description = $"获取词条详情，其中包含关联条目，可以通过Id再次获取关联条目的详情。\"{string.Join(",",SearchInputModel.GetEntryTypes().Skip(1).Select(s=>s.GetDisplayName()))}\"都属于词条。",
                        Parameters = new ChatCompletionParameters
                        {
                            Properties = new Dictionary<string, ChatCompletionProperty>
                            {
                                ["id"] = new ChatCompletionProperty
                                {
                                    Type = "integer",
                                    Description = "词条id，例如：1"
                                }
                            },
                            Required = ["id"]
                        }
                    }
                },
                new ChatCompletionTool
                {
                    Function = new ChatCompletionFunction
                    {
                        Name = "get_article",
                        Description = $"获取文章详情，其中包含关联条目，可以通过Id再次获取关联条目的详情。\"{string.Join(",",SearchInputModel.GetArticleTypes().Skip(1).Select(s=>s.GetDisplayName()))}\"都属于文章",
                        Parameters = new ChatCompletionParameters
                        {
                            Properties = new Dictionary<string, ChatCompletionProperty>
                            {
                                ["id"] = new ChatCompletionProperty
                                {
                                    Type = "integer",
                                    Description = "词条id，例如：1"
                                }
                            },
                            Required = ["id"]
                        }
                    }
                },
                new ChatCompletionTool
                {
                    Function = new ChatCompletionFunction
                    {
                        Name = "get_latest_games",
                        Description = "获取最近发售的游戏",
                        Parameters = new ChatCompletionParameters
                        {
                            Properties = [],
                            Required = []
                        }
                    }
                },
                new ChatCompletionTool
                {
                    Function = new ChatCompletionFunction
                    {
                        Name = "get_latest_demo_games",
                        Description = "获取最近试玩的游戏",
                        Parameters = new ChatCompletionParameters
                        {
                            Properties = [],
                            Required = []
                        }
                    }
                },
                new ChatCompletionTool
                {
                    Function = new ChatCompletionFunction
                    {
                        Name = "get_upcoming_games",
                        Description = "获取即将发售的游戏",
                        Parameters = new ChatCompletionParameters
                        {
                            Properties = [],
                            Required = []
                        }
                    }
                },
                new ChatCompletionTool
                {
                    Function = new ChatCompletionFunction
                    {
                        Name = "get_news",
                        Description = "获取CnGal资料站收录的制作组/STAFF的最新动态",
                        Parameters = new ChatCompletionParameters
                        {
                            Properties = [],
                            Required = []
                        }
                    }
                }
                // 在这里可以添加更多工具
            ];
        }

        public async Task<string> ExecuteFunction(string functionName, string arguments)
        {
            _logger.LogInformation("调用函数：{}，参数：{}", functionName, arguments);
            if (_functionMap.TryGetValue(functionName, out var function))
            {
                return await function(arguments);
            }

            throw new ArgumentException($"未找到名为 {functionName} 的函数");
        }

        /// <summary>
        /// 获取最近发售的游戏
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private async Task<string> GetLatestGames(string arguments)
        {
            var result = await _httpService.GetAsync<List<PublishedGameItemModel>>($"{_apiUrl}api/home/ListPublishedGames");

            if (result == null || result.Count == 0)
            {
                return "最近没有发售的游戏";
            }

            return JsonSerializer.Serialize(result.Select(s => new
            {
                s.Name,
                s.BriefIntroduction,
                s.Tags,
                id = ExtractIdFromUrl(s.Url)
            }), _jsonOptions);
        }

        /// <summary>
        /// 获取最近试玩的游戏
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private async Task<string> GetLatestDemoGames(string arguments)
        {
            var result = await _httpService.GetAsync<List<RecentlyDemoGameItemModel>>($"{_apiUrl}api/home/ListRecentlyDemoGames");

            if (result == null || result.Count == 0)
            {
                return "最近没有试玩的游戏";
            }

            return JsonSerializer.Serialize(result.Select(s => new
            {
                s.Name,
                s.BriefIntroduction,
                s.Tags,
                id = ExtractIdFromUrl(s.Url)
            }), _jsonOptions);
        }

        /// <summary>
        /// 获取即将发行的游戏
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private async Task<string> GetUpcomingGames(string arguments)
        {
            var result = await _httpService.GetAsync<List<UpcomingGameItemModel>>($"{_apiUrl}api/home/ListUpcomingGames");

            if (result == null || result.Count == 0)
            {
                return "没有即将发售的游戏";
            }

            return JsonSerializer.Serialize(result.Select(s => new
            {
                s.Name,
                s.BriefIntroduction,
                s.PublishTime,
                id = ExtractIdFromUrl(s.Url)
            }), _jsonOptions);
        }

        /// <summary>
        /// 获取动态
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private async Task<string> GetNews(string arguments)
        {
            var result = await _httpService.GetAsync<List<HomeNewsAloneViewModel>>($"{_apiUrl}api/home/GetHomeNewsView");

            if (result == null || result.Count == 0)
            {
                return "最近没有制作组动态";
            }

            return JsonSerializer.Serialize(result.Select(s => new
            {
                s.Title,
                s.Text,
                Time = s.Time.ToString("d"),
                s.ArticleId
            }), _jsonOptions);
        }

        /// <summary>
        /// 获取词条详细信息
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private async Task<string> GetEntry(string arguments)
        {
            var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(arguments);
            if (args == null)
            {
                throw new ArgumentException("缺少参数");
            }

            // 词条id
            int entryId = 1;
            if (args.TryGetValue("id", out var pageElement))
            {
                switch (pageElement.ValueKind)
                {
                    case JsonValueKind.Number:
                        entryId = pageElement.GetInt32();
                        break;
                    case JsonValueKind.String:
                        if (!int.TryParse(pageElement.GetString(), out entryId))
                        {
                            throw new ArgumentException("无法解析参数 id");
                        }
                        break;
                    default:
                        throw new ArgumentException("无法解析参数 id");
                }
            }

            // 访问cngal
            try
            {
                var result = await _httpService.GetAsync<EntryIndexViewModel>($"{_apiUrl}api/entries/GetEntryView/{entryId}?renderMarkdown=false");

                if (result == null)
                {
                    return "找不到该词条";
                }

                return JsonSerializer.Serialize(new
                {
                    result.Id,
                    result.Name,
                    result.AnotherName,
                    brief = result.BriefIntroduction,
                    result.ProductionGroups,
                    Informations = result.Information.Select(s => new
                    {
                        s.Name,
                        s.Value
                    }),
                    result.Staffs,
                    Roles = result.Roles.Select(s => new
                    {
                        s.Id,
                        s.Name,
                        brief = s.BriefIntroduction,
                        s.CV,
                        s.Birthday,
                    }),
                    StaffGames = result.StaffGames.Select(s => new
                    {
                        s.Id,
                        s.Name,
                        brief = s.BriefIntroduction,
                        s.AddInfors,
                    }),
                    entry_relevances = result.EntryRelevances.Select(s => new
                    {
                        s.Id,
                        s.Name,
                        brief = s.BriefIntroduction,
                        type = s.Type.GetDisplayName(),
                    }),
                    article_relevances = result.ArticleRelevances.Select(s => new
                    {
                        s.Id,
                        s.Name,
                        s.CreateUserName,
                        brief = s.BriefIntroduction,
                        type = s.Type.GetDisplayName(),
                    }),
                    result.Tags,
                    result.Releases
                }, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取词条数据失败");
                return "找不到该词条";
            }
        }

        /// <summary>
        /// 获取文章详细信息
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private async Task<string> GetArticle(string arguments)
        {
            var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(arguments);
            if (args == null)
            {
                throw new ArgumentException("缺少参数");
            }

            // 词条id
            int articleId = 1;
            if (args.TryGetValue("id", out var pageElement))
            {
                switch (pageElement.ValueKind)
                {
                    case JsonValueKind.Number:
                        articleId = pageElement.GetInt32();
                        break;
                    case JsonValueKind.String:
                        if (!int.TryParse(pageElement.GetString(), out articleId))
                        {
                            throw new ArgumentException("无法解析参数 id");
                        }
                        break;
                    default:
                        throw new ArgumentException("无法解析参数 id");
                }
            }

            // 访问cngal
            try
            {
                var result = await _httpService.GetAsync<ArticleViewModel>($"{_apiUrl}api/articles/GetArticleView/{articleId}?renderMarkdown=false");

                if (result == null)
                {
                    return "找不到该文章";
                }

                return JsonSerializer.Serialize(new
                {
                    result.Id,
                    result.Name,
                    author = result.UserInfor?.Name,
                    result.MainPage,
                    entry_relevances = result.RelatedEntries.Select(s => new
                    {
                        s.Id,
                        s.Name,
                        brief = s.BriefIntroduction,
                        type = s.Type.GetDisplayName(),
                    }),
                    article_relevances = result.RelatedArticles.Select(s => new
                    {
                        s.Id,
                        s.Name,
                        s.CreateUserName,
                        brief = s.BriefIntroduction,
                        type = s.Type.GetDisplayName(),
                    }),
                }, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取词条数据失败");
                return "找不到该词条";
            }
        }

        /// <summary>
        /// 搜索CnGal
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private async Task<string> SearchCnGal(string arguments)
        {
            var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(arguments);
            if (args == null)
            {
                throw new ArgumentException("缺少参数");
            }

            // 搜索文本
            if (!args.TryGetValue("text", out var textElement))
            {
                throw new ArgumentException("缺少搜索文本参数");
            }
            string text = textElement.GetString() ?? throw new ArgumentException("搜索文本不能为空");

            // 当前页数
            int current_page = 1;
            if (args.TryGetValue("current_page", out var pageElement))
            {
                switch (pageElement.ValueKind)
                {
                    case JsonValueKind.Number:
                        current_page = pageElement.GetInt32();
                        break;
                    case JsonValueKind.String:
                        if (!int.TryParse(pageElement.GetString(), out current_page))
                        {
                            current_page = 1;
                        }
                        break;
                }
            }

            // 访问cngal
            SearchInputModel InputModel = new SearchInputModel();
            InputModel.FilterText = text;
            InputModel.CurrentPage = current_page;

            var result = await _httpService.GetAsync<SearchViewModel>($"{_apiUrl}api/home/search{InputModel.ToQueryParameterString()}");

            if (result == null || result.pagedResultDto.TotalCount == 0)
            {
                return "搜索不到结果";
            }

            // 拼接文本
            var model = new SearchModel
            {
                CurrentPage = result.pagedResultDto.CurrentPage,
                TotalCount = result.pagedResultDto.TotalCount,
                TotalPage = result.pagedResultDto.TotalPages,
            };
            foreach (var item in result.pagedResultDto.Data)
            {
                if (item.entry != null)
                {
                    model.Items.Add(new SearchItemModel
                    {
                        BriefIntroduction = item.entry.BriefIntroduction,
                        Name = item.entry.Name,
                        SubType = item.entry.Type.GetDisplayName(),
                        MainType = "词条",
                        Id = item.entry.Id,
                    });
                }
                else if (item.article != null)
                {
                    model.Items.Add(new SearchItemModel
                    {
                        BriefIntroduction = item.article.BriefIntroduction,
                        Name = item.article.Name,
                        SubType = item.article.Type.GetDisplayName(),
                        MainType = "文章",
                        Id = item.article.Id,
                    });
                }
                else if (item.tag != null)
                {
                    model.Items.Add(new SearchItemModel
                    {
                        BriefIntroduction = item.tag.BriefIntroduction,
                        Name = item.tag.Name,
                        MainType = "标签",
                        Id = item.tag.Id,
                    });
                }
                else if (item.periphery != null)
                {
                    model.Items.Add(new SearchItemModel
                    {
                        BriefIntroduction = item.periphery.BriefIntroduction,
                        Name = item.periphery.Name,
                        SubType = item.periphery.Type.GetDisplayName(),
                        MainType = "周边",
                        Id = item.periphery.Id,
                    });
                }
                else if (item.video != null)
                {
                    model.Items.Add(new SearchItemModel
                    {
                        BriefIntroduction = item.video.BriefIntroduction,
                        Name = item.video.Name,
                        SubType = item.video.Type,
                        MainType = "视频",
                        Id = item.video.Id,
                    });
                }
            }

            return JsonSerializer.Serialize(model, _jsonOptions);
        }
    }
}
