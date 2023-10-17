using CnGalWebSite.RobotClientX.Models.Messages;
using CnGalWebSite.RobotClientX.Models.Robots;
using CnGalWebSite.RobotClientX.DataRepositories;
using CnGalWebSite.RobotClientX.Extentions;
using CnGalWebSite.RobotClientX.Services.Events;
using CnGalWebSite.RobotClientX.Services.Messages;
using Masuda.Net;
using MeowMiraiLib;
using MeowMiraiLib.Msg.Sender;
using MeowMiraiLib.Msg.Type;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Message = MeowMiraiLib.Msg.Type.Message;
using CnGalWebSite.RobotClientX.DataModels;
using CnGalWebSite.EventBus.Services;
using RabbitMQ.Client.Events;

namespace CnGalWebSite.RobotClientX.Services.QQClients
{
    public class QQClientService : IQQClientService, IDisposable
    {
        private readonly IRepository<RobotGroup> _robotGroupRepository;
        private readonly IRepository<RobotEvent> _robotEventRepository;
        private readonly IRepository<PostLog> _postLogRepository;
        private readonly IConfiguration _configuration;
        private readonly IMessageService _messageService;
        private readonly IEventService _eventService;
        private readonly IEventBusService _eventBusService;
        private readonly ILogger<QQClientService> _logger;
        public MasudaBot MasudaClient { get; set; }
        public Client MiraiClient { get; set; }
        private string _miraiSession;

        System.Timers.Timer t = new(1000 * 60);
        System.Timers.Timer t2 = new(1000 * 60);

        public QQClientService(IRepository<RobotGroup> robotGroupRepository, IRepository<PostLog> postLogRepository,
            IConfiguration configuration, ILogger<QQClientService> logger, IEventService eventService, IEventBusService eventBusService,
        IMessageService messageService, IRepository<RobotEvent> robotEventRepository)
        {
            _robotGroupRepository = robotGroupRepository;
            _configuration = configuration;
            _messageService = messageService;
            _postLogRepository = postLogRepository;
            _logger = logger;
            _eventService = eventService;
            _robotEventRepository = robotEventRepository;
            _eventBusService = eventBusService;
        }

        public void InitMasuda()
        {
            if (int.TryParse(_configuration["ChannelAppId"], out int appId) == false)
            {
                _logger.LogError("初始化频道失败，ChannelAppId无效");
                return;
            }
            try
            {
                MasudaClient = new MasudaBot(appId, _configuration["ChannelAppKey"], _configuration["ChannelToken"], BotType.Private);

                _logger.LogInformation("成功初始化 Masuda 客户端");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化 Masuda 客户端失败，配置文件错误或 依赖项 未正确启动");
            }

        }

        public async Task InitMirai()
        {
            try
            {
                MiraiClient = new($"ws://{_configuration["MiraiUrl"]}/all?verifyKey={_configuration["NormalVerifyKey"]}&qq={_configuration["QQ"]}");
                MiraiClient._OnServeiceConnected += MiraiClient__OnServeiceConnected; ;
                if (await MiraiClient.ConnectAsync())
                {
                    _logger.LogInformation("成功初始化 Mirai 客户端");
                }
                else
                {
                    _logger.LogError("初始化 Mirai 客户端失败");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化 Mirai 客户端失败");
            }

        }

        public void InitEventBus()
        {
            if (string.IsNullOrWhiteSpace(_configuration["EventBus_HostName"]) == false)
            {
                _eventBusService.RecieveQQMessage(async (e) =>
                {
                    await SendMessage(RobotReplyRange.Friend, e.QQ, e.Message);
                });
                _eventBusService.RecieveQQGroupMessage(async (e) =>
                {
                    await SendMessage(RobotReplyRange.Group, e.GroupId, e.Message);
                });
            }
        }

        private void MiraiClient__OnServeiceConnected(string e)
        {

            if (string.IsNullOrWhiteSpace(e))
            {
                return;
            }
            var session = e.MidStrEx("\"session\": \"", "\"");
            if (string.IsNullOrWhiteSpace(session))
            {
                return;
            }
            _miraiSession = session;
            _logger.LogInformation("成功连接Mirai服务器，Session：{Session}", session);
        }

        public async Task Init()
        {
            //初始化QQ群
            await InitMirai();
            //初始化事件总线
            InitEventBus();

            if (MiraiClient != null)
            {

                //定时任务计时器
                t.Start(); //启动计时器
                t.Elapsed += async (s, e) =>
                {
                    try
                    {
                        var message = _eventService.GetCurrentTimeEvent();
                        if (string.IsNullOrWhiteSpace(message) == false)
                        {
                            var result = await _messageService.ProcMessageAsync(RobotReplyRange.Group, message, "", null, 0, null);

                            if (result != null)
                            {
                                foreach (var item in _robotGroupRepository.GetAll().Where(s => s.IsHidden == false && s.ForceMatch == false))
                                {
                                    result.SendTo = item.GroupId;
                                    await SendMessage(result);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "定时任务异常");
                    }

                };

                //随机任务计时器
                t2.Start(); //启动计时器
                t2.Elapsed += async (s, e) =>
                {
                    try
                    {
                        var message = _eventService.GetProbabilityEvents();
                        if (string.IsNullOrWhiteSpace(message) == false)
                        {
                            var result = await _messageService.ProcMessageAsync(RobotReplyRange.Group, message, "", null, 0, null);

                            if (result != null)
                            {
                                foreach (var item in _robotGroupRepository.GetAll().Where(s => s.IsHidden == false && s.ForceMatch == false))
                                {
                                    result.SendTo = item.GroupId;
                                    await SendMessage(result);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "随机任务异常");
                    }

                };

                //好友消息事件
                MiraiClient.OnFriendMessageReceive += async (s, e) =>
                {
                    try
                    {
                        await ReplyFromFriendAsync(s, e);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "无法回复好友消息");
                    }

                };


                //群聊消息事件
                MiraiClient.OnGroupMessageReceive += async (s, e) =>
                {
                    try
                    {
                        await ReplyFromGroupAsync(s, e);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "无法回复群聊消息");
                    }

                };
            }


            //初始化频道

            InitMasuda();


            if (MasudaClient != null)
            {
                // 普通消息事件注册(需要私域)
                MasudaClient.MessageAction += async (bot, msg, type) =>
                 {
                     try
                     {
                         await ReplyFromChannelAsync(msg);
                     }
                     catch (Exception ex)
                     {
                         _logger.LogError(ex, "回复消息失败");
                     }
                 };
            }

            _logger.LogInformation("CnGal资料站 看板娘 v3.4.8");

        }

        /// <summary>
        /// 回复频道消息
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public async Task ReplyFromChannelAsync(Masuda.Net.Models.Message msg)
        {
            //过滤非水区的发言
            if ((await MasudaClient.GetChannelAsync(msg.ChannelId)).Name.Contains("水区") == false)
            {
                return;
            }

            var messgae = MessageHelper.GetPureMessage(msg.Content);
            if (string.IsNullOrWhiteSpace(messgae))
            {
                return;
            }
            await ReplyMessageAsync(RobotReplyRange.Channel, MessageHelper.GetPureMessage(msg.Content), 0, 0, msg.Author.Username, msg);

        }

        /// <summary>
        /// 回复好友消息
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public async Task ReplyFromFriendAsync(FriendMessageSender s, MeowMiraiLib.Msg.Type.Message[] msg)
        {
            await ReplyMessageAsync(RobotReplyRange.Friend, ConversionMeaasge(msg), s.id, s.id, s.nickname);
        }

        /// <summary>
        /// 回复群聊消息
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public async Task ReplyFromGroupAsync(GroupMessageSender s, MeowMiraiLib.Msg.Type.Message[] msg)
        {
            //忽略未关注的群聊
            RobotGroup group = _robotGroupRepository.GetAll().FirstOrDefault(x => x.GroupId == s.group.id && x.IsHidden == false);
            if (group == null)
            {
                return;
            }
            //检查是否符合强制匹配
            string message = ConversionMeaasge(msg);
            if (group.ForceMatch)
            {
                string name = _configuration["RobotName"] ?? "看板娘";
                if ((name != null && message.Contains(name) == false) || message.Contains("介绍") == false)
                {
                    return;
                }
            }
            await ReplyMessageAsync(RobotReplyRange.Group, message, s.group.id, s.id, s.memberName);
        }

        /// <summary>
        /// 转换消息
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private string ConversionMeaasge(MeowMiraiLib.Msg.Type.Message[] e)
        {
            string message = e.MGetPlainString();
            Message at = e.FirstOrDefault(s => s.type == "At");
            if (at != null)
            {
                long atTarget = (at as At).target;
                message += "[@" + atTarget.ToString() + "]";
            }
            return message;
        }

        /// <summary>
        /// 回复消息
        /// </summary>
        private async Task ReplyMessageAsync(RobotReplyRange range, string message, long sendto, long memberId, string memberName, Masuda.Net.Models.Message msg = null)
        {
            //尝试找出所有匹配的回复
            RobotReply reply = _messageService.GetAutoReply(message, range);

            if (reply == null)
            {
                return;
            }
            SendMessageModel result = new();


            try
            {
                //处理消息
                result = await _messageService.ProcMessageAsync(range, reply.Value, message, reply.Key, memberId, memberName);
            }
            catch (ArgError ae)
            {
                if (long.TryParse(_configuration["WarningQQGroup"], out long warningQQGroup))
                {
                    //发送警告
                    await SendMessage(new SendMessageModel { SendTo = warningQQGroup, MiraiMessage = new MeowMiraiLib.Msg.Type.Message[] { new Plain(ae.Error) }, Range = RobotReplyRange.Group });

                }
            }


            if (result == null)
            {
                return;
            }

            //添加发送记录 并不代表真实发送
            _ = _postLogRepository.Insert(new PostLog
            {
                Message = message,
                PostTime = DateTime.Now.ToCstTime(),
                QQ = memberId,
                Reply = reply.Value
            });

            //检查上限
            if (await CheckLimit(range, sendto, memberId, memberName, msg))
            {
                return;
            }

            //发送消息
            result.SendTo = sendto;
            await SendMessage(result, msg);


        }

        /// <summary>
        /// 检查是否超过限制
        /// </summary>
        /// <param name="sendto"></param>
        /// <param name="memberId"></param>
        /// <param name="memberName"></param>
        /// <returns>是否超过限制</returns>
        private async Task<bool> CheckLimit(RobotReplyRange range, long sendto, long memberId, string memberName, Masuda.Net.Models.Message msg = null)
        {
            //判断该用户是否连续10次互动 1分钟内
            int singleCount = _postLogRepository.GetAll().Count(x => (DateTime.Now.ToCstTime() - x.PostTime).TotalMinutes <= 1 && x.QQ == memberId);
            int totalCount = _postLogRepository.GetAll().Count(x => (DateTime.Now.ToCstTime() - x.PostTime).TotalMinutes <= 1);

            //读取上限次数配置
            if (!long.TryParse(_configuration["SingleLimit"], out long singleLimit))
            {
                singleLimit = 5;
            }
            if (!long.TryParse(_configuration["TotalLimit"], out long totalLimit))
            {
                totalLimit = 10;
            }

            //检查上限
            if (singleCount == singleLimit)
            {
                SendMessageModel result = await _messageService.ProcMessageAsync(range, _robotEventRepository.GetAll().FirstOrDefault(s => s.Note == "消息上限警告")?.Text ?? $"[image=https://image.cngal.org/kanbanFace/hhzywx.png][@{memberId}]如果恶意骚扰人家的话，我会请你离开哦…", null, null, memberId, memberName);
                result.SendTo = sendto;
                await SendMessage(result, msg);
                return true;
            }

            if (totalCount == totalLimit)
            {
                SendMessageModel result = await _messageService.ProcMessageAsync(range, $"核心温度过高，正在冷却......", null, null, memberId, memberName);
                result.SendTo = sendto;
                await SendMessage(result, msg);
                return true;
            }

            if (singleCount > singleLimit)
            {
                return true;
            }

            if (totalCount > totalLimit)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="range"></param>
        /// <param name="id"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public async Task SendMessage(RobotReplyRange range, long sendto, string text, Masuda.Net.Models.Message msg = null)
        {
            if (range == RobotReplyRange.Channel)
            {
                await SendMessage(new SendMessageModel
                {
                    Text = text,
                    MasudaMessage = _messageService.ProcMessageToMasuda(text),
                    Range = range,
                }, msg);
            }
            else
            {
                await SendMessage(new SendMessageModel
                {
                    Text = text,
                    SendTo = sendto,
                    MiraiMessage = _messageService.ProcMessageToMirai(text),
                    Range = range,
                });
            }

        }

        public async Task SendMessage(SendMessageModel model, Masuda.Net.Models.Message msg = null)
        {

            if (model.MiraiMessage == null && model.MasudaMessage == null)
            {
                return;
            }


            if (model.Range == RobotReplyRange.Channel)
            {
                if (model.MasudaMessage == null)
                {
                    return;
                }

                _ = await MasudaClient.ReplyMessageAsync(msg, model.MasudaMessage);

                _logger.LogInformation("向频道发送“{消息}”", model.Text);

            }
            else if (model.Range == RobotReplyRange.Friend)
            {

                model.MiraiMessage.SendToFriend(model.SendTo, MiraiClient);

                _logger.LogInformation("向 {group} 发送“{消息}”成功", model.SendTo, model.Text);
            }
            else
            {
                model.MiraiMessage.SendToGroup(model.SendTo, MiraiClient);
                _logger.LogInformation("向 {group} 发送“{消息}”成功", model.SendTo, model.Text);
            }

            return;
        }

        public void Dispose()
        {
            if (t != null)
            {
                t.Dispose();
                t = null;
            }
            if (t2 != null)
            {
                t2.Dispose();
                t2 = null;
            }
        }

        public string GetMiraiSession()
        {
            return _miraiSession;
        }
    }
}
