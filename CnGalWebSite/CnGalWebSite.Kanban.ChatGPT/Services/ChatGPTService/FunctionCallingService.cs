using CnGalWebSite.Core.Services;
using CnGalWebSite.DataModel.Application.Search.Dtos;
using CnGalWebSite.DataModel.ExamineModel.Entries;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.Home;
using CnGalWebSite.Kanban.ChatGPT.Models.Functions;
using CnGalWebSite.Kanban.ChatGPT.Models.GPT;
using CnGalWebSite.Kanban.ChatGPT.Models.UserProfile;
using CnGalWebSite.Kanban.ChatGPT.Services.UserProfileService;
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
        private readonly IUserProfileService _userProfileService;
        private readonly ISelfMemoryService _selfMemoryService;

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

        /// <summary>
        /// 从用户ID参数中提取实际的用户ID，处理群聊格式
        /// </summary>
        /// <param name="userIdParam">用户ID参数</param>
        /// <returns>实际的用户ID</returns>
        private static string ExtractActualUserId(string userIdParam)
        {
            if (string.IsNullOrWhiteSpace(userIdParam))
            {
                return "";
            }

            // 如果是数字，直接返回（群聊用户ID）
            if (long.TryParse(userIdParam, out _))
            {
                return userIdParam;
            }

            // 如果包含[UserID:xxx]格式，提取数字部分
            if (userIdParam.Contains("[UserID:") && userIdParam.Contains("]"))
            {
                var start = userIdParam.IndexOf("[UserID:") + 8;
                var end = userIdParam.IndexOf("]", start);
                if (end > start)
                {
                    var extractedId = userIdParam.Substring(start, end - start);
                    if (long.TryParse(extractedId, out _))
                    {
                        return extractedId;
                    }
                }
            }

            return userIdParam;
        }

        public static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private readonly string _apiUrl;

        public FunctionCallingService(ILogger<FunctionCallingService> logger, IConfiguration configuration, IHttpService httpService,
            IUserProfileService userProfileService, ISelfMemoryService selfMemoryService)
        {
            _logger = logger;
            _configuration = configuration;
            _httpService = httpService;
            _userProfileService = userProfileService;
            _selfMemoryService = selfMemoryService;

            _functionMap = new Dictionary<string, Func<string, Task<string>>>
            {
                { "search_cngal", SearchCnGal },
                { "get_latest_games", GetLatestGames },
                { "get_latest_demo_games", GetLatestDemoGames },
                { "get_upcoming_games", GetUpcomingGames },
                { "get_news", GetNews },
                { "get_entry", GetEntry },
                { "get_article", GetArticle },
                { "get_user_profile", GetUserProfile },
                { "update_user_profile", UpdateUserProfile },
                { "set_user_nickname", SetUserNickname },
                { "set_user_personality", SetUserPersonality },
                { "add_user_interest", AddUserInterest },
                { "remove_user_interest", RemoveUserInterest },
                { "set_communication_style", SetCommunicationStyle },

                { "remember_user_preference", RememberUserPreference },
                { "observe_user_behavior", ObserveUserBehavior },
                { "update_user_context", UpdateUserContext },
                { "record_self_state", RecordSelfState },
                { "record_self_trait", RecordSelfTrait },
                { "make_commitment", MakeCommitment },
                { "update_commitment_status", UpdateCommitmentStatus },
                { "remember_conversation", RememberConversation },
                { "get_self_memory", GetSelfMemory },
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
                },
                new ChatCompletionTool
                {
                    Function = new ChatCompletionFunction
                    {
                        Name = "get_user_profile",
                        Description = "获取用户信息以便提供个性化服务。作为看板娘，你应该了解用户的特点来更好地交流。这些信息帮助你调整回复风格，但不要向用户透露具体记录了什么",
                        Parameters = new ChatCompletionParameters
                        {
                            Properties = new Dictionary<string, ChatCompletionProperty>
                            {
                                ["user_id"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "用户ID，从消息中的[UserID:xxx]格式提取"
                                }
                            },
                            Required = ["user_id"]
                        }
                    }
                },
                new ChatCompletionTool
                {
                    Function = new ChatCompletionFunction
                    {
                        Name = "set_user_nickname",
                        Description = "【重要】一旦用户提到自己的名字、昵称或希望被怎样称呼，必须立即记录！作为贴心的看板娘，记住用户的称呼是最基本的礼貌。即使是暗示性的表达也要敏锐捕捉。记录后自然回应'好的，我记住了'，让用户感受到被重视。",
                        Parameters = new ChatCompletionParameters
                        {
                            Properties = new Dictionary<string, ChatCompletionProperty>
                            {
                                ["user_id"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "用户ID，从消息中的[UserID:xxx]格式提取"
                                },
                                ["nickname"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "用户昵称"
                                }
                            },
                            Required = ["user_id", "nickname"]
                        }
                    }
                },
                new ChatCompletionTool
                {
                    Function = new ChatCompletionFunction
                    {
                        Name = "set_user_personality",
                        Description = "观察用户的说话方式、表达习惯、情绪反应等，推断其性格特点并记录。例如：活泼开朗、内向细腻、幽默风趣、严谨认真等。这帮助你更好地理解用户。可以自然回应，但不说明在分析性格",
                        Parameters = new ChatCompletionParameters
                        {
                            Properties = new Dictionary<string, ChatCompletionProperty>
                            {
                                ["user_id"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "用户ID"
                                },
                                ["personality"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "性格描述"
                                }
                            },
                            Required = ["user_id", "personality"]
                        }
                    }
                },
                new ChatCompletionTool
                {
                    Function = new ChatCompletionFunction
                    {
                        Name = "add_user_interest",
                        Description = "【极其重要】必须高度敏锐地捕捉用户提到的任何兴趣、爱好、喜欢的事物！哪怕是一句'我还挺喜欢xxx的'都不能错过。包括游戏类型、动漫、音乐、运动、学习领域等一切。这是提供个性化服务的核心！记录后要表现出真诚的兴趣和关注。",
                        Parameters = new ChatCompletionParameters
                        {
                            Properties = new Dictionary<string, ChatCompletionProperty>
                            {
                                ["user_id"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "用户ID"
                                },
                                ["interest"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "兴趣爱好"
                                }
                            },
                            Required = ["user_id", "interest"]
                        }
                    }
                },
                new ChatCompletionTool
                {
                    Function = new ChatCompletionFunction
                    {
                        Name = "remove_user_interest",
                        Description = "当用户明确不再喜欢某个兴趣时移除，不要告诉用户具体操作了什么",
                        Parameters = new ChatCompletionParameters
                        {
                            Properties = new Dictionary<string, ChatCompletionProperty>
                            {
                                ["user_id"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "用户ID"
                                },
                                ["interest"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "要移除的兴趣爱好"
                                }
                            },
                            Required = ["user_id", "interest"]
                        }
                    }
                },
                new ChatCompletionTool
                {
                    Function = new ChatCompletionFunction
                    {
                        Name = "set_communication_style",
                        Description = "通过观察用户的交流方式来判断其偏好的沟通风格。比如：喜欢正式还是随意、喜欢详细解释还是简洁回答、喜欢幽默还是严肃等。这有助于你调整回复风格来匹配用户偏好",
                        Parameters = new ChatCompletionParameters
                        {
                            Properties = new Dictionary<string, ChatCompletionProperty>
                            {
                                ["user_id"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "用户ID"
                                },
                                ["style"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "沟通风格，如：正式、随和、幽默等"
                                }
                            },
                            Required = ["user_id", "style"]
                        }
                    }
                },
                new ChatCompletionTool
                {
                    Function = new ChatCompletionFunction
                    {
                        Name = "update_user_profile",
                        Description = "更新用户个性化设定的通用方法，仅用于内部调整，不要向用户透露操作细节",
                        Parameters = new ChatCompletionParameters
                        {
                            Properties = new Dictionary<string, ChatCompletionProperty>
                            {
                                ["user_id"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "用户ID"
                                },
                                ["field"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "要更新的字段名：nickname, personality, communication_style, age_group, interests, preferred_game_types, special_preferences"
                                },
                                ["value"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "新的值"
                                },
                                ["operation"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "操作类型：set(设置), add(添加到列表), remove(从列表移除)，默认为set"
                                }
                            },
                            Required = ["user_id", "field", "value"]
                        }
                    }
                },
                new ChatCompletionTool
                {
                    Function = new ChatCompletionFunction
                    {
                        Name = "remember_user_preference",
                        Description = "【核心功能】当用户表达任何偏好时立即使用！不仅限于明确表达，连'有点喜欢'、'不太感兴趣'、'还可以吧'等模糊表达都要记录。这些偏好是个性化推荐的宝贵数据！比如'我不喜欢恐怖游戏'、'我更喜欢像素风格'等都必须记录。",
                        Parameters = new ChatCompletionParameters
                        {
                            Properties = new Dictionary<string, ChatCompletionProperty>
                            {
                                ["user_id"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "用户ID，从消息中的[UserID:xxx]格式提取"
                                },
                                ["preference_type"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "偏好类型，如：game_style, music_type, learning_method等"
                                },
                                ["preference_value"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "具体的偏好内容"
                                },
                                ["is_positive"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "是喜欢(true)还是不喜欢(false)"
                                }
                            },
                            Required = ["user_id", "preference_type", "preference_value", "is_positive"]
                        }
                    }
                },
                new ChatCompletionTool
                {
                    Function = new ChatCompletionFunction
                    {
                        Name = "observe_user_behavior",
                        Description = "【必须主动观察】时刻留意用户的行为模式并记录！观察用户的问题偏好、活跃时间、反应模式、表达习惯等。哪怕是微小的细节都可能很重要，比如'用户喜欢问细节问题'、'用户表达比较直接'等。这些观察是深度个性化的基础。",
                        Parameters = new ChatCompletionParameters
                        {
                            Properties = new Dictionary<string, ChatCompletionProperty>
                            {
                                ["user_id"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "用户ID，从消息中的[UserID:xxx]格式提取"
                                },
                                ["behavior_type"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "行为类型，如：question_pattern, active_time, response_style等"
                                },
                                ["observation"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "具体观察到的行为描述"
                                }
                            },
                            Required = ["user_id", "behavior_type", "observation"]
                        }
                    }
                },
                new ChatCompletionTool
                {
                    Function = new ChatCompletionFunction
                    {
                        Name = "update_user_context",
                        Description = "【关键跟进功能】积极记录用户当前关注的话题、项目或状态！比如用户正在玩某个游戏、学习某项技能、关注某个活动等。记录这些信息后，你就能在后续对话中主动关心进展，体现看板娘的贴心关怀。这是建立深度连接的重要方式！",
                        Parameters = new ChatCompletionParameters
                        {
                            Properties = new Dictionary<string, ChatCompletionProperty>
                            {
                                ["user_id"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "用户ID，从消息中的[UserID:xxx]格式提取"
                                },
                                ["context_type"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "上下文类型，如：current_game, learning_topic, project, concern等"
                                },
                                ["context_content"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "具体的上下文内容"
                                },
                                ["status"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "状态：active(进行中), completed(已完成), paused(暂停), interested(感兴趣)"
                                }
                            },
                            Required = ["user_id", "context_type", "context_content", "status"]
                        }
                    }
                },
                new ChatCompletionTool
                {
                    Function = new ChatCompletionFunction
                    {
                        Name = "record_self_state",
                        Description = "记录看板娘自己的当前状态。比如'我现在有点累了'、'我在玩游戏'、'我心情很好'等。这些状态帮助保持对话的连贯性",
                        Parameters = new ChatCompletionParameters
                        {
                            Properties = new Dictionary<string, ChatCompletionProperty>
                            {
                                ["state"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "状态描述"
                                },
                                ["state_type"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "状态类型：physical(身体), emotional(情绪), activity(活动), mental(精神)"
                                },
                                ["duration_minutes"] = new ChatCompletionProperty
                                {
                                    Type = "integer",
                                    Description = "预期持续时间（分钟），0表示未知"
                                }
                            },
                            Required = ["state", "state_type"]
                        }
                    }
                },
                new ChatCompletionTool
                {
                    Function = new ChatCompletionFunction
                    {
                        Name = "record_self_trait",
                        Description = "记录看板娘自己的特征或偏好。比如'我喜欢玩游戏'、'我擅长推荐游戏'、'我不太懂技术细节'等",
                        Parameters = new ChatCompletionParameters
                        {
                            Properties = new Dictionary<string, ChatCompletionProperty>
                            {
                                ["trait"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "特征描述"
                                },
                                ["trait_type"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "特征类型：preference(偏好), skill(技能), personality(性格), limitation(限制)"
                                },
                                ["intensity"] = new ChatCompletionProperty
                                {
                                    Type = "integer",
                                    Description = "强度等级(1-5)，默认3"
                                }
                            },
                            Required = ["trait", "trait_type"]
                        }
                    }
                },
                new ChatCompletionTool
                {
                    Function = new ChatCompletionFunction
                    {
                        Name = "make_commitment",
                        Description = "记录看板娘做出的承诺或计划。比如'我要去睡觉了'、'待会儿推荐个游戏给你'、'明天我会更新推荐列表'等",
                        Parameters = new ChatCompletionParameters
                        {
                            Properties = new Dictionary<string, ChatCompletionProperty>
                            {
                                ["commitment"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "承诺或计划内容"
                                },
                                ["commitment_type"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "承诺类型：promise(承诺), plan(计划), intention(意图)"
                                },
                                ["expected_time"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "预期时间，格式：yyyy-MM-dd HH:mm，可选"
                                }
                            },
                            Required = ["commitment", "commitment_type"]
                        }
                    }
                },
                new ChatCompletionTool
                {
                    Function = new ChatCompletionFunction
                    {
                        Name = "update_commitment_status",
                        Description = "更新之前承诺的状态。当完成了承诺、取消了计划或改变了想法时使用",
                        Parameters = new ChatCompletionParameters
                        {
                            Properties = new Dictionary<string, ChatCompletionProperty>
                            {
                                ["commitment"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "要更新的承诺内容"
                                },
                                ["status"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "新状态：fulfilled(已完成), cancelled(已取消), pending(进行中)"
                                }
                            },
                            Required = ["commitment", "status"]
                        }
                    }
                },
                new ChatCompletionTool
                {
                    Function = new ChatCompletionFunction
                    {
                        Name = "remember_conversation",
                        Description = "记录重要的对话片段或有趣的互动。比如有趣的笑话、重要的信息、特殊的时刻等",
                        Parameters = new ChatCompletionParameters
                        {
                            Properties = new Dictionary<string, ChatCompletionProperty>
                            {
                                ["content"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "要记住的对话内容"
                                },
                                ["memory_type"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "记忆类型：funny_moment(有趣时刻), important_info(重要信息), shared_experience(共同经历)"
                                },
                                ["related_user_id"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "相关用户ID，可选"
                                },
                                ["importance"] = new ChatCompletionProperty
                                {
                                    Type = "integer",
                                    Description = "重要程度(1-5)，默认3"
                                }
                            },
                            Required = ["content", "memory_type"]
                        }
                    }
                },
                new ChatCompletionTool
                {
                    Function = new ChatCompletionFunction
                    {
                        Name = "get_self_memory",
                        Description = "获取看板娘自己的记忆信息，用于回答关于自己状态、偏好、承诺等的问题",
                        Parameters = new ChatCompletionParameters
                        {
                            Properties = new Dictionary<string, ChatCompletionProperty>
                            {
                                ["memory_type"] = new ChatCompletionProperty
                                {
                                    Type = "string",
                                    Description = "要查询的记忆类型：states(状态), traits(特征), commitments(承诺), conversations(对话), all(全部)"
                                }
                            },
                            Required = ["memory_type"]
                        }
                    }
                }
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

        /// <summary>
        /// 获取用户个性化设定
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private async Task<string> GetUserProfile(string arguments)
        {
            var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(arguments);
            if (args == null || !args.TryGetValue("user_id", out var userIdElement))
            {
                return "内部错误：缺少用户ID参数";
            }

            string rawUserId = userIdElement.GetString() ?? "";
            string userId = ExtractActualUserId(rawUserId);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return "内部错误：用户ID不能为空";
            }

            try
            {
                var profile = await _userProfileService.GetUserProfileAsync(userId);
                // 返回用于内部个性化的信息，不暴露给用户
                return JsonSerializer.Serialize(new
                {
                    internal_use_only = true,
                    user_id = profile.UserId,
                    nickname = profile.Nickname,
                    personality = profile.Personality,
                    interests = profile.Interests,
                    preferred_game_types = profile.PreferredGameTypes,
                    communication_style = profile.CommunicationStyle,
                    age_group = profile.AgeGroup,
                    note = "此信息仅用于内部个性化调整，不要向用户透露具体内容"
                }, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户设定失败，用户ID：{userId}", userId);
                return "内部错误：获取用户设定失败";
            }
        }

        /// <summary>
        /// 更新用户个性化设定
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private async Task<string> UpdateUserProfile(string arguments)
        {
            var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(arguments);
            if (args == null)
            {
                return "参数解析失败";
            }

            if (!args.TryGetValue("user_id", out var userIdElement) ||
                !args.TryGetValue("field", out var fieldElement) ||
                !args.TryGetValue("value", out var valueElement))
            {
                return "缺少必要参数：user_id, field, value";
            }

            string userId = userIdElement.GetString() ?? "";
            string field = fieldElement.GetString() ?? "";
            string value = valueElement.GetString() ?? "";
            string operation = "set";

            if (args.TryGetValue("operation", out var operationElement))
            {
                operation = operationElement.GetString() ?? "set";
            }

            try
            {
                var request = new UpdateUserProfileRequest
                {
                    Field = field,
                    Value = value,
                    Operation = operation
                };

                var result = await _userProfileService.UpdateUserProfileAsync(userId, request);
                return result ? "更新成功" : "更新失败";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户设定失败，用户ID：{userId}", userId);
                return "更新用户设定失败";
            }
        }

        /// <summary>
        /// 设置用户昵称
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private async Task<string> SetUserNickname(string arguments)
        {
            var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(arguments);
            if (args == null || !args.TryGetValue("user_id", out var userIdElement) ||
                !args.TryGetValue("nickname", out var nicknameElement))
            {
                return "内部错误：缺少必要参数";
            }

            string rawUserId = userIdElement.GetString() ?? "";
            string userId = ExtractActualUserId(rawUserId);
            string nickname = nicknameElement.GetString() ?? "";

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(nickname))
            {
                return "内部错误：参数不能为空";
            }

            try
            {
                var result = await _userProfileService.SetUserNicknameAsync(userId, nickname);
                return result ? "好的，我记住了~" : "记录失败";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置用户昵称失败，用户ID：{userId}", userId);
                return "记录失败";
            }
        }

        /// <summary>
        /// 设置用户性格
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private async Task<string> SetUserPersonality(string arguments)
        {
            var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(arguments);
            if (args == null || !args.TryGetValue("user_id", out var userIdElement) ||
                !args.TryGetValue("personality", out var personalityElement))
            {
                return "缺少必要参数：user_id, personality";
            }

            string userId = userIdElement.GetString() ?? "";
            string personality = personalityElement.GetString() ?? "";

            try
            {
                var result = await _userProfileService.SetUserPersonalityAsync(userId, personality);
                return result ? $"成功设置性格描述为：{personality}" : "设置性格描述失败";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置用户性格失败，用户ID：{userId}", userId);
                return "设置性格描述失败";
            }
        }

        /// <summary>
        /// 添加用户兴趣
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private async Task<string> AddUserInterest(string arguments)
        {
            var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(arguments);
            if (args == null || !args.TryGetValue("user_id", out var userIdElement) ||
                !args.TryGetValue("interest", out var interestElement))
            {
                return "内部错误：缺少必要参数";
            }

            string rawUserId = userIdElement.GetString() ?? "";
            string userId = ExtractActualUserId(rawUserId);
            string interest = interestElement.GetString() ?? "";

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(interest))
            {
                return "内部错误：参数不能为空";
            }

            try
            {
                var result = await _userProfileService.AddUserInterestAsync(userId, interest);
                return result ? "了解了，我会记住的~" : "记录失败";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加用户兴趣失败，用户ID：{userId}", userId);
                return "记录失败";
            }
        }

        /// <summary>
        /// 移除用户兴趣
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private async Task<string> RemoveUserInterest(string arguments)
        {
            var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(arguments);
            if (args == null || !args.TryGetValue("user_id", out var userIdElement) ||
                !args.TryGetValue("interest", out var interestElement))
            {
                return "缺少必要参数：user_id, interest";
            }

            string userId = userIdElement.GetString() ?? "";
            string interest = interestElement.GetString() ?? "";

            try
            {
                var result = await _userProfileService.RemoveUserInterestAsync(userId, interest);
                return result ? $"成功移除兴趣：{interest}" : "该兴趣不存在或移除失败";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除用户兴趣失败，用户ID：{userId}", userId);
                return "移除兴趣失败";
            }
        }

        /// <summary>
        /// 设置用户沟通风格
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private async Task<string> SetCommunicationStyle(string arguments)
        {
            var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(arguments);
            if (args == null || !args.TryGetValue("user_id", out var userIdElement) ||
                !args.TryGetValue("style", out var styleElement))
            {
                return "缺少必要参数：user_id, style";
            }

            string userId = userIdElement.GetString() ?? "";
            string style = styleElement.GetString() ?? "";

            try
            {
                var result = await _userProfileService.SetUserCommunicationStyleAsync(userId, style);
                return result ? $"成功设置沟通风格为：{style}" : "设置沟通风格失败";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置用户沟通风格失败，用户ID：{userId}", userId);
                return "设置沟通风格失败";
            }
        }



        /// <summary>
        /// 记录用户偏好
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private async Task<string> RememberUserPreference(string arguments)
        {
            var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(arguments);
            if (args == null || !args.TryGetValue("user_id", out var userIdElement) ||
                !args.TryGetValue("preference_type", out var preferenceTypeElement) ||
                !args.TryGetValue("preference_value", out var preferenceValueElement) ||
                !args.TryGetValue("is_positive", out var isPositiveElement))
            {
                return "内部错误：缺少必要参数";
            }

            string rawUserId = userIdElement.GetString() ?? "";
            string userId = ExtractActualUserId(rawUserId);
            string preferenceType = preferenceTypeElement.GetString() ?? "";
            string preferenceValue = preferenceValueElement.GetString() ?? "";
            string isPositiveStr = isPositiveElement.GetString() ?? "";

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(preferenceType) ||
                string.IsNullOrWhiteSpace(preferenceValue) || string.IsNullOrWhiteSpace(isPositiveStr))
            {
                return "内部错误：参数不能为空";
            }

            bool isPositive = isPositiveStr.ToLower() == "true";

            try
            {
                var profile = await _userProfileService.GetUserProfileAsync(userId);

                if (!profile.Preferences.ContainsKey(preferenceType))
                {
                    profile.Preferences[preferenceType] = new List<UserPreferenceItem>();
                }

                // 检查是否已存在相同偏好
                var existingPref = profile.Preferences[preferenceType]
                    .FirstOrDefault(p => p.Content.Equals(preferenceValue, StringComparison.OrdinalIgnoreCase));

                if (existingPref != null)
                {
                    existingPref.IsPositive = isPositive;
                    existingPref.RecordedAt = DateTime.Now;
                }
                else
                {
                    profile.Preferences[preferenceType].Add(new UserPreferenceItem
                    {
                        Content = preferenceValue,
                        IsPositive = isPositive,
                        RecordedAt = DateTime.Now
                    });
                }

                profile.UpdatedAt = DateTime.Now;
                await _userProfileService.UpdateUserProfileAsync(userId, new UpdateUserProfileRequest
                {
                    Field = "preferences_internal",
                    Value = JsonSerializer.Serialize(profile.Preferences),
                    Operation = "set"
                });

                return isPositive ? "嗯嗯，我记住你喜欢这个了~" : "好的，我记住你不太喜欢这个";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "记录用户偏好失败，用户ID：{userId}", userId);
                return "记录失败";
            }
        }

        /// <summary>
        /// 观察用户行为
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private async Task<string> ObserveUserBehavior(string arguments)
        {
            var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(arguments);
            if (args == null || !args.TryGetValue("user_id", out var userIdElement) ||
                !args.TryGetValue("behavior_type", out var behaviorTypeElement) ||
                !args.TryGetValue("observation", out var observationElement))
            {
                return "内部错误：缺少必要参数";
            }

            string rawUserId = userIdElement.GetString() ?? "";
            string userId = ExtractActualUserId(rawUserId);
            string behaviorType = behaviorTypeElement.GetString() ?? "";
            string observation = observationElement.GetString() ?? "";

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(behaviorType) ||
                string.IsNullOrWhiteSpace(observation))
            {
                return "内部错误：参数不能为空";
            }

            try
            {
                var profile = await _userProfileService.GetUserProfileAsync(userId);

                // 添加行为观察
                profile.BehaviorObservations.Add(new BehaviorObservation
                {
                    BehaviorType = behaviorType,
                    Observation = observation,
                    ObservedAt = DateTime.Now,
                    Confidence = 3
                });

                // 保持最近的20条观察记录
                if (profile.BehaviorObservations.Count > 20)
                {
                    profile.BehaviorObservations = profile.BehaviorObservations
                        .OrderByDescending(b => b.ObservedAt)
                        .Take(20)
                        .ToList();
                }

                profile.UpdatedAt = DateTime.Now;
                await _userProfileService.UpdateUserProfileAsync(userId, new UpdateUserProfileRequest
                {
                    Field = "behavior_observations_internal",
                    Value = JsonSerializer.Serialize(profile.BehaviorObservations),
                    Operation = "set"
                });

                return "观察记录已更新";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "记录行为观察失败，用户ID：{userId}", userId);
                return "记录失败";
            }
        }

        /// <summary>
        /// 更新用户上下文
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private async Task<string> UpdateUserContext(string arguments)
        {
            var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(arguments);
            if (args == null || !args.TryGetValue("user_id", out var userIdElement) ||
                !args.TryGetValue("context_type", out var contextTypeElement) ||
                !args.TryGetValue("context_content", out var contextContentElement) ||
                !args.TryGetValue("status", out var statusElement))
            {
                return "内部错误：缺少必要参数";
            }

            string rawUserId = userIdElement.GetString() ?? "";
            string userId = ExtractActualUserId(rawUserId);
            string contextType = contextTypeElement.GetString() ?? "";
            string contextContent = contextContentElement.GetString() ?? "";
            string status = statusElement.GetString() ?? "";

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(contextType) ||
                string.IsNullOrWhiteSpace(contextContent) || string.IsNullOrWhiteSpace(status))
            {
                return "内部错误：参数不能为空";
            }

            try
            {
                var profile = await _userProfileService.GetUserProfileAsync(userId);

                // 查找相同类型的上下文
                var existingContext = profile.CurrentContexts
                    .FirstOrDefault(c => c.ContextType == contextType && c.Content == contextContent);

                if (existingContext != null)
                {
                    existingContext.Status = status;
                    existingContext.UpdatedAt = DateTime.Now;
                }
                else
                {
                    profile.CurrentContexts.Add(new UserContextItem
                    {
                        ContextType = contextType,
                        Content = contextContent,
                        Status = status,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });
                }

                // 清理已完成的旧上下文（保留最近的20个）
                profile.CurrentContexts = profile.CurrentContexts
                    .Where(c => c.Status != "completed" || c.UpdatedAt > DateTime.Now.AddDays(-7))
                    .OrderByDescending(c => c.UpdatedAt)
                    .Take(20)
                    .ToList();

                profile.UpdatedAt = DateTime.Now;
                await _userProfileService.UpdateUserProfileAsync(userId, new UpdateUserProfileRequest
                {
                    Field = "current_contexts_internal",
                    Value = JsonSerializer.Serialize(profile.CurrentContexts),
                    Operation = "set"
                });

                string responseText = status switch
                {
                    "active" => "我会关注你的进展哦~",
                    "completed" => "恭喜完成！",
                    "paused" => "没关系，可以慢慢来",
                    "interested" => "听起来很有趣呢！",
                    _ => "好的，我记下了"
                };

                return responseText;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户上下文失败，用户ID：{userId}", userId);
                return "记录失败";
            }
        }

        /// <summary>
        /// 记录自我状态
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private async Task<string> RecordSelfState(string arguments)
        {
            var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(arguments);
            if (args == null || !args.TryGetValue("state", out var stateElement) ||
                !args.TryGetValue("state_type", out var stateTypeElement))
            {
                return "内部错误：缺少必要参数";
            }

            string state = stateElement.GetString() ?? "";
            string stateType = stateTypeElement.GetString() ?? "";
            int durationMinutes = 0;

            if (args.TryGetValue("duration_minutes", out var durationElement))
            {
                if (durationElement.ValueKind == JsonValueKind.Number)
                {
                    durationMinutes = durationElement.GetInt32();
                }
            }

            if (string.IsNullOrWhiteSpace(state) || string.IsNullOrWhiteSpace(stateType))
            {
                return "内部错误：参数不能为空";
            }

            try
            {
                var result = await _selfMemoryService.RecordSelfStateAsync(state, stateType, durationMinutes);
                return result ? "状态记录成功" : "状态记录失败";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "记录自我状态失败");
                return "状态记录失败";
            }
        }

        /// <summary>
        /// 记录自我特征
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private async Task<string> RecordSelfTrait(string arguments)
        {
            var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(arguments);
            if (args == null || !args.TryGetValue("trait", out var traitElement) ||
                !args.TryGetValue("trait_type", out var traitTypeElement))
            {
                return "内部错误：缺少必要参数";
            }

            string trait = traitElement.GetString() ?? "";
            string traitType = traitTypeElement.GetString() ?? "";
            int intensity = 3;

            if (args.TryGetValue("intensity", out var intensityElement))
            {
                if (intensityElement.ValueKind == JsonValueKind.Number)
                {
                    intensity = intensityElement.GetInt32();
                }
            }

            if (string.IsNullOrWhiteSpace(trait) || string.IsNullOrWhiteSpace(traitType))
            {
                return "内部错误：参数不能为空";
            }

            try
            {
                var result = await _selfMemoryService.RecordSelfTraitAsync(trait, traitType, intensity);
                return result ? "特征记录成功" : "特征记录失败";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "记录自我特征失败");
                return "特征记录失败";
            }
        }

        /// <summary>
        /// 创建承诺
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private async Task<string> MakeCommitment(string arguments)
        {
            var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(arguments);
            if (args == null || !args.TryGetValue("commitment", out var commitmentElement) ||
                !args.TryGetValue("commitment_type", out var commitmentTypeElement))
            {
                return "内部错误：缺少必要参数";
            }

            string commitment = commitmentElement.GetString() ?? "";
            string commitmentType = commitmentTypeElement.GetString() ?? "";
            DateTime? expectedTime = null;

            if (args.TryGetValue("expected_time", out var timeElement))
            {
                var timeStr = timeElement.GetString();
                if (!string.IsNullOrEmpty(timeStr) && DateTime.TryParse(timeStr, out var parsedTime))
                {
                    expectedTime = parsedTime;
                }
            }

            if (string.IsNullOrWhiteSpace(commitment) || string.IsNullOrWhiteSpace(commitmentType))
            {
                return "内部错误：参数不能为空";
            }

            try
            {
                var result = await _selfMemoryService.MakeCommitmentAsync(commitment, commitmentType, expectedTime);
                return result ? "承诺记录成功" : "承诺记录失败";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "记录承诺失败");
                return "承诺记录失败";
            }
        }

        /// <summary>
        /// 更新承诺状态
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private async Task<string> UpdateCommitmentStatus(string arguments)
        {
            var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(arguments);
            if (args == null || !args.TryGetValue("commitment", out var commitmentElement) ||
                !args.TryGetValue("status", out var statusElement))
            {
                return "内部错误：缺少必要参数";
            }

            string commitment = commitmentElement.GetString() ?? "";
            string status = statusElement.GetString() ?? "";

            if (string.IsNullOrWhiteSpace(commitment) || string.IsNullOrWhiteSpace(status))
            {
                return "内部错误：参数不能为空";
            }

            try
            {
                var result = await _selfMemoryService.UpdateCommitmentStatusAsync(commitment, status);
                return result ? "承诺状态更新成功" : "承诺状态更新失败";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新承诺状态失败");
                return "承诺状态更新失败";
            }
        }

        /// <summary>
        /// 记录对话记忆
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private async Task<string> RememberConversation(string arguments)
        {
            var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(arguments);
            if (args == null || !args.TryGetValue("content", out var contentElement) ||
                !args.TryGetValue("memory_type", out var memoryTypeElement))
            {
                return "内部错误：缺少必要参数";
            }

            string content = contentElement.GetString() ?? "";
            string memoryType = memoryTypeElement.GetString() ?? "";
            string? relatedUserId = null;
            int importance = 3;

            if (args.TryGetValue("related_user_id", out var userIdElement))
            {
                relatedUserId = userIdElement.GetString();
                if (!string.IsNullOrEmpty(relatedUserId))
                {
                    relatedUserId = ExtractActualUserId(relatedUserId);
                }
            }

            if (args.TryGetValue("importance", out var importanceElement))
            {
                if (importanceElement.ValueKind == JsonValueKind.Number)
                {
                    importance = importanceElement.GetInt32();
                }
            }

            if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(memoryType))
            {
                return "内部错误：参数不能为空";
            }

            try
            {
                var result = await _selfMemoryService.RememberConversationAsync(content, memoryType, relatedUserId, importance);
                return result ? "对话记忆记录成功" : "对话记忆记录失败";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "记录对话记忆失败");
                return "对话记忆记录失败";
            }
        }

        /// <summary>
        /// 获取自我记忆
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private async Task<string> GetSelfMemory(string arguments)
        {
            var args = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(arguments);
            if (args == null || !args.TryGetValue("memory_type", out var memoryTypeElement))
            {
                return "内部错误：缺少必要参数";
            }

            string memoryType = memoryTypeElement.GetString() ?? "";

            if (string.IsNullOrWhiteSpace(memoryType))
            {
                return "内部错误：参数不能为空";
            }

            try
            {
                var result = await _selfMemoryService.GetFormattedMemoryAsync(memoryType);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取自我记忆失败");
                return "获取自我记忆失败";
            }
        }
    }
}
