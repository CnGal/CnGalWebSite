using CnGalWebSite.Core.Models;
using CnGalWebSite.Core.Services;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Robots;
using CnGalWebSite.Kanban.Components.Dialogs;
using CnGalWebSite.Kanban.Models;
using CnGalWebSite.Kanban.Services.Core;
using CnGalWebSite.PublicToolbox.Extentions;
using CnGalWebSite.Shared.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core.Tokenizer;
using System.Text;
using System.Threading.Tasks;
using Result = CnGalWebSite.DataModel.Model.Result;

namespace CnGalWebSite.Kanban.Services.Chat
{
    public class ChatService : IChatService
    {
        private List<ChatModel> _chats = new List<ChatModel>();
        private ChatCard _chatCard;


        public bool IsOpen { get; set; }

        private readonly IHttpService _httpService;
        private readonly ILive2DService _live2DService;
        private readonly IDataCacheService _dataCacheService;

        public ChatService(IHttpService httpService, ILive2DService live2DService, IDataCacheService dataCacheService)
        {
            _httpService = httpService;
            _live2DService = live2DService;
            _dataCacheService = dataCacheService;
        }

        public void Init(ChatCard chatCard)
        {
            _chatCard = chatCard;
        }

        public void Show()
        {
            IsOpen = true;
            _chatCard?.ShowCard();
        }

        public void Close()
        {
            IsOpen = false;
        }



        public void Add(ChatModel model)
        {
            _chats.Add(model);
        }

        public void Clear()
        {
            _chats.Clear();
        }


        public async Task GetChatReply(GetKanbanReplyModel model)
        {
            var re = await _httpService.PostAsync<GetKanbanReplyModel, Result>("api/robot/GetKanbanReply/", model);
            if (re.Successful == false)
            {
                throw new Exception(re.Error);
            }

            Add(new ChatModel
            {
                Image = await _live2DService.GetKanbanImageGeneration(),
                Text = re.Error,
                Time = DateTime.Now.ToCstTime(),
                IsUser = false,
            });
        }


        /// <summary>
        /// 获取聊天记录
        /// </summary>
        /// <returns></returns>
        public async Task<List<ChatModel>> GetChatRecords(DeviceIdentificationModel Identification)
        {
            // 判断消息是否过期
            if (_chats.Count != 0 && _chats.Last().Time.AddDays(1) < DateTime.Now.ToCstTime())
            {
                _chats.Clear();
            }

            // 判断是否第一次聊天
            if (_chats.Count == 0)
            {
                await GetChatReply(new GetKanbanReplyModel
                {
                    Identification = Identification,
                    IsFirst = true
                });
                return _chats;
            }

            return _chats;
        }
    }
}
