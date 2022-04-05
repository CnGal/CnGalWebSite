using System;
using System.IO;
using Senparc.Weixin.MP.MessageHandlers;
using Senparc.Weixin.MP.Entities;
using Senparc.NeuChar.Context;
using Senparc.NeuChar.Entities;
using Senparc.Weixin.MP.MessageContexts;
using Senparc.CO2NET.Utilities;
using Senparc.NeuChar.Entities.Request;
using Senparc.NeuChar.Helpers;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP;
using Senparc.Weixin;
using System.Linq;
using System.Threading.Tasks;
using Senparc.CO2NET.Helpers;
using CnGalWebSite.APIServer.Application.WeiXin;
using CnGalWebSite.APIServer.DataReositories;
using CnGalWebSite.DataModel.Model;
using Microsoft.Extensions.DependencyInjection;

namespace CnGalWebSite.APIServer.MessageHandlers
{
    public partial class CustomMessageHandler : MessageHandler<DefaultMpMessageContext>
    {
        /*
 * 重要提示：v1.5起，MessageHandler提供了一个DefaultResponseMessage的抽象方法，
 * DefaultResponseMessage必须在子类中重写，用于返回没有处理过的消息类型（也可以用于默认消息，如帮助信息等）；
 * 其中所有原OnXX的抽象方法已经都改为虚方法，可以不必每个都重写。若不重写，默认返回DefaultResponseMessage方法中的结果。
 */
        private string appId = Config.SenparcWeixinSetting.MpSetting.WeixinAppId;
        private string appSecret = Config.SenparcWeixinSetting.MpSetting.WeixinAppSecret;

        /// <summary>
        /// 为中间件提供生成当前类的委托
        /// </summary>
        public static Func<Stream, PostModel, int, IServiceProvider, CustomMessageHandler> GenerateMessageHandler = (stream, postModel, maxRecordCount, serviceProvider)
                         => new CustomMessageHandler(stream, postModel, maxRecordCount, false /* 是否只允许处理加密消息，以提高安全性 */, serviceProvider: serviceProvider);

        /// <summary>
        /// 自定义 MessageHandler
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="postModel"></param>
        /// <param name="maxRecordCount"></param>
        /// <param name="onlyAllowEncryptMessage"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="weiXinService"></param>
        public CustomMessageHandler(Stream inputStream, PostModel postModel, int maxRecordCount = 0, bool onlyAllowEncryptMessage = false, IServiceProvider serviceProvider = null )
            : base(inputStream, postModel, maxRecordCount, onlyAllowEncryptMessage, serviceProvider: serviceProvider)
        {
            //这里设置仅用于测试，实际开发可以在外部更全局的地方设置，
            //比如MessageHandler<MessageContext>.GlobalGlobalMessageContext.ExpireMinutes = 3。
            GlobalMessageContext.ExpireMinutes = 3;
            
            OnlyAllowEncryptMessage = true;//是否只允许接收加密消息，默认为 false

        }


        /// <summary>
        /// 处理文字请求
        /// </summary>
        /// <param name="requestMessage">请求消息</param>
        /// <returns></returns>
        public override async Task<IResponseMessageBase> OnTextRequestAsync(RequestMessageText requestMessage)
        {
            var defaultResponseMessage = base.CreateResponseMessage<ResponseMessageText>();
            using var scope = ServiceProvider.CreateScope();
            var _weiXinService = scope.ServiceProvider.GetRequiredService<IWeiXinService>();

            // 下方 RequestHandler 为消息关键字提供了便捷的处理方式，当然您也可以使用传统的 if...else... 对 requestMessage.Content 进行判断
            var requestHandler = await requestMessage.StartHandler()
                //关键字不区分大小写，按照顺序匹配成功后将不再运行下面的逻辑
                //正则表达式
                .Regex(@"^词条", () =>
                {
                    var temp= requestMessage.Content.Replace("词条","").Replace("词条 ", "");
                    int id = 0;
                    if (int.TryParse(temp, out id)&&id>0)
                    {
                        defaultResponseMessage.Content = _weiXinService.GetEntryInfor(id).GetAwaiter().GetResult();
                    }
                    else
                    {
                        defaultResponseMessage.Content = "呜~~~ 看板娘看不懂呜~";
                    }

                    return defaultResponseMessage;
                })
                .Regex(@"^搜索", () =>
                {
                    var temp = requestMessage.Content.Replace("搜索", "").Replace("搜索 ", "");
                    if (string.IsNullOrWhiteSpace(temp)==false)
                    {
                        defaultResponseMessage.Content = _weiXinService.GetSearchResults(temp).GetAwaiter().GetResult();
                    }
                    else
                    {
                        defaultResponseMessage.Content = "呜~~~ 看板娘看不懂呜~";
                    }

                    return defaultResponseMessage;
                })
                .Keywords(new string[] { "随机推荐" }, () =>
                {
                    defaultResponseMessage.Content =  _weiXinService.GetRandom().GetAwaiter().GetResult();

                    return defaultResponseMessage;
                })
                .Keywords(new string[] { "最新动态" }, () =>
                {
                    defaultResponseMessage.Content = _weiXinService.GetNewestNews().GetAwaiter().GetResult();

                    return defaultResponseMessage;
                })
                .Keywords(new string[] { "近期新作" }, () =>
                {
                    defaultResponseMessage.Content = _weiXinService.GetNewestPublishGames().GetAwaiter().GetResult();

                    return defaultResponseMessage;
                })
                .Keywords(new string[] { "即将发售" }, () =>
                {
                    defaultResponseMessage.Content = _weiXinService.GetNewestUnPublishGames().GetAwaiter().GetResult();

                    return defaultResponseMessage;
                })
                .Keywords(new string[] { "最新编辑" }, () =>
                {
                    defaultResponseMessage.Content = _weiXinService.GetNewestEditGames().GetAwaiter().GetResult();

                    return defaultResponseMessage;
                })
                .Keywords(new string[] { "关于我们", "关于" }, () =>
                {
                    var articleResponseMessage = base.CreateResponseMessage<ResponseMessageNews>();
                    articleResponseMessage.Articles = new System.Collections.Generic.List<Senparc.NeuChar.Entities.Article>
                    {
                        new Senparc.NeuChar.Entities.Article
                        {
                            PicUrl="https://image.cngal.org/images/2022/02/16/95d8f31d4d94.png",
                            Description="CnGal是一个非营利性的，立志于收集整理国内制作组创作及中文化的中文Galgame/AVG的介绍、攻略、评测、感想等内容的资料性质的网站。 此外，CnGal官方还会与圈内中文AVG制作组进行友好合作，如免费提供Banner广告位，网站服务器资源等。",
                            Title="CnGal 中文GalGame资料站",
                            Url="https://www.cngal.org/about"
                        }
                    };

                    return articleResponseMessage;
                })
                .Keywords(new string[] { "戳戳看板娘" , "戳戳","看板娘" }, () =>
                {
                    defaultResponseMessage.Content = _weiXinService.GetAboutUsage();

                    return defaultResponseMessage;
                })


                //当 Default 使用异步方法时，需要写在最后一个，且 requestMessage.StartHandler() 前需要使用 await 等待异步方法执行；
                //当 Default 使用同步方法，不一定要在最后一个,并且不需要使用 await
                .Default(async () =>
                {
                    defaultResponseMessage.Content = $"看板娘不知道哦~ 欸嘿嘿~~~";
                    return defaultResponseMessage;
                });

            return requestHandler.GetResponseMessage() as IResponseMessageBase;
        }

        public override async Task<IResponseMessageBase> DefaultResponseMessageAsync(IRequestMessageBase requestMessage)
        {
            return DefaultResponseMessage(requestMessage);
        }



        public override IResponseMessageBase DefaultResponseMessage(IRequestMessageBase requestMessage)
        {
            /* 所有没有被处理的消息会默认返回这里的结果，
            * 因此，如果想把整个微信请求委托出去（例如需要使用分布式或从其他服务器获取请求），
            * 只需要在这里统一发出委托请求，如：
            * var responseMessage = MessageAgent.RequestResponseMessage(agentUrl, agentToken, RequestDocument.ToString());
            * return responseMessage;
            */

            var responseMessage = this.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = $"这条消息来自DefaultResponseMessage。\r\n您收到这条消息，表明该公众号没有对【{requestMessage.MsgType}】类型做处理。";
            return responseMessage;
        }


        public override async Task<IResponseMessageBase> OnUnknownTypeRequestAsync(RequestMessageUnknownType requestMessage)
        {
            /*
             * 此方法用于应急处理SDK没有提供的消息类型，
             * 原始XML可以通过requestMessage.RequestDocument（或this.RequestDocument）获取到。
             * 如果不重写此方法，遇到未知的请求类型将会抛出异常（v14.8.3 之前的版本就是这么做的）
             */
            var msgType = Senparc.NeuChar.Helpers.MsgTypeHelper.GetRequestMsgTypeString(requestMessage.RequestDocument);
            var responseMessage = this.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "未知消息类型：" + msgType;

            WeixinTrace.SendCustomLog("未知请求消息类型", requestMessage.RequestDocument.ToString());//记录到日志中

            return responseMessage;
        }


        /// <summary>
        /// 处理事件请求（这个方法一般不用重写，这里仅作为示例出现。除非需要在判断具体Event类型以外对Event信息进行统一操作
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override async Task<IResponseMessageBase> OnEventRequestAsync(IRequestMessageEventBase requestMessage)
        {
            var eventResponseMessage = await base.OnEventRequestAsync(requestMessage);//对于Event下属分类的重写方法，见：CustomerMessageHandler_Events.cs
                                                                                      //TODO: 对Event信息进行统一操作
            return eventResponseMessage;
        }


    }
}
