using Blazored.LocalStorage;
using CnGalWebSite.Kanban.Models;
using CnGalWebSite.Kanban.Services.Core;
using CnGalWebSite.Kanban.Services.Dialogs;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.Services.Events
{
    public class EventService : IEventService, IDisposable
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IDialogBoxService _dialogBoxService;
        private readonly ILogger<EventService> _logger;
        private readonly NavigationManager _navigationManager;
        private DotNetObjectReference<EventService>? objRef;
        private readonly IJSRuntime _jSRuntime;

        private Random _random;
        private EventGroupModel _eventGroupModel = new();

        private const string _localKey = "kanban_events";
        private const string _remoteKey = "EventGroup.json";

        public EventGroupModel EventGroup
        {
            get
            {
                return _eventGroupModel;
            }
        }

        public EventService(ILocalStorageService localStorageService, HttpClient httpClient, IConfiguration configuration, IDialogBoxService dialogBoxService, ILogger<EventService> logger, NavigationManager navigationManager, IJSRuntime jSRuntime)
        {
            _localStorageService = localStorageService;
            _httpClient = httpClient;
            _configuration = configuration;
            _dialogBoxService = dialogBoxService;
            _logger = logger;
            _navigationManager = navigationManager;
            _random = new Random(DateTime.Now.Microsecond);
            _jSRuntime = jSRuntime;
        }

        private async Task LoadAsync()
        {
            try
            {
                var localData = await _localStorageService.GetItemAsync<EventGroupModel>(_localKey);
                if (localData == null)
                {
                    localData = new EventGroupModel();
                }
                _eventGroupModel = await _httpClient.GetFromJsonAsync<EventGroupModel>(_configuration["Live2D_DataUrl"] + _remoteKey);

                //复制最后执行时间
                CopyData(localData.CustomEvents, _eventGroupModel.CustomEvents);
                CopyData(localData.MouseOverEvents, _eventGroupModel.MouseOverEvents);
                CopyData(localData.TimeEvents, _eventGroupModel.TimeEvents);
                CopyData(localData.DateEvents, _eventGroupModel.DateEvents);
                CopyData(localData.IdleEvents, _eventGroupModel.IdleEvents);

                //保存数据
                await SaveAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取事件数据失败");
            }
        }

        /// <summary>
        /// 初始化鼠标悬停事件
        /// </summary>
        /// <returns></returns>
        public async Task InitMouseOverEvent()
        {
            //创建对象引用
            objRef = DotNetObjectReference.Create(this);

            await _jSRuntime.InvokeVoidAsync("initMouseOverEvent", objRef, _eventGroupModel.MouseOverEvents.Select(s => new
            {
                s.Selector,
                s.Id
            }));
        }

        /// <summary>
        /// 触发鼠标悬停事件
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        [JSInvokable]
        public async Task TriggerMouseOverEventAsync(int id)
        {
            var item = _eventGroupModel.MouseOverEvents.FirstOrDefault(s => s.Id == id);
            if (item != null)
            {
                await TriggerEventAsync(item, new CancellationTokenSource().Token);
            }
        }

        /// <summary>
        /// 触发自定义事件
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [JSInvokable]
        public async Task TriggerCustomEventAsync(string name)
        {
            var item = _eventGroupModel.CustomEvents.FirstOrDefault(s => s.Name == name);
            if (item != null)
            {
                await TriggerEventAsync(item, new CancellationTokenSource().Token);
            }
        }

        /// <summary>
        /// 触发事件
        /// </summary>
        /// <param name="model"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task TriggerEventAsync(BaseEventModel model, CancellationToken token)
        {
            try
            {
                var content = model.Contents[_random.Next(0, model.Contents.Count)];
                _dialogBoxService.ShowDialogBox(content,model.Priority);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "触发事件失败");
            }
            model.LastTriggerTime = DateTime.Now;
            await SaveAsync();
        }

        /// <summary>
        /// 运行任务监听
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task RunAsync(CancellationToken token)
        {
            //加载数据
            await LoadAsync();
            //订阅事件
            _navigationManager.LocationChanged -= LocationChanged;
            _navigationManager.LocationChanged += LocationChanged;
            //初始化鼠标悬停
            await InitMouseOverEvent();
            //初始化完毕
            await Task.Delay(2000);
            //循环
            while (true)
            {
                var now = DateTime.Now;
                var list = new List<BaseEventModel>();
                //日期
                list.AddRange(_eventGroupModel.DateEvents
                   .Where(s => !(s.LastTriggerTime.Year == now.Year && s.LastTriggerTime.Month == now.Month && s.LastTriggerTime.Day == now.Day))
                   .Where(s => s.Date.Month == now.Month && s.Date.Day == now.Day));

                //时间
                var timeNow = TimeOnly.FromDateTime(now);
                list.AddRange(_eventGroupModel.TimeEvents
                   .Where(s => !(s.LastTriggerTime.Year == now.Year && s.LastTriggerTime.Month == now.Month && s.LastTriggerTime.Day == now.Day))
                   .Where(s => timeNow >= s.AfterTime && timeNow <= s.BeforeTime));

                //空闲
                if (_random.Next(0, 10000) < 10 && _eventGroupModel.IdleEvents.Any())
                {
                    var index = _random.Next(0, _eventGroupModel.IdleEvents.Count());
                    list.Add(_eventGroupModel.IdleEvents[index]);
                }

                //触发事件
                foreach (var item in list)
                {
                    await TriggerEventAsync(item, token);

                    //取消
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                }

                //取消
                if (token.IsCancellationRequested)
                {
                    return;
                }

                await Task.Delay(100, token);
            }
        }

        /// <summary>
        /// 导航事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private async void LocationChanged(object sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
        {
            var url = _navigationManager.Uri.Replace(_navigationManager.BaseUri, "");
            var item = _eventGroupModel.NavigationEvents.FirstOrDefault(s => s.Url == url);
            if (item != null)
            {
                await TriggerEventAsync(item, new CancellationTokenSource().Token);
            }
        }

        /// <summary>
        /// 复制最后执行时间
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="des"></param>
        public void CopyData<T>(List<T> src, List<T> des) where T : BaseEventModel
        {
            foreach (var item in src)
            {
                var data = des.FirstOrDefault(s => s.Id == item.Id);
                if (data != null)
                {
                    data.LastTriggerTime = item.LastTriggerTime;
                }
            }
        }

        /// <summary>
        /// 保存最后执行时间
        /// </summary>
        /// <returns></returns>
        public async Task SaveAsync()
        {
            //保存
            await _localStorageService.SetItemAsync(_localKey, _eventGroupModel);
        }

        public void Dispose()
        {
            objRef?.Dispose();
            _navigationManager.LocationChanged -= LocationChanged;
        }
    }
}
