using Blazored.LocalStorage;
using CnGalWebSite.MainSite.Shared.Services.KanbanModels;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace CnGalWebSite.MainSite.Shared.Services;

public sealed class KanbanEventService : IKanbanEventService, IAsyncDisposable
{
    private readonly ILocalStorageService _localStorage;
    private readonly IKanbanRemoteConfigService _remoteConfig;
    private readonly IJSRuntime _jsRuntime;
    private readonly NavigationManager _navigation;
    private readonly ILogger<KanbanEventService> _logger;

    private KanbanEventGroupModel? _eventGroup;
    private KanbanEventTrackingState _tracking = new();
    private DateTime _lastInteractionTime = DateTime.Now;
    private DateTime _lastIdleCheckTime = DateTime.MinValue;
    private readonly List<KanbanDialogBoxModel> _dialogQueue = new(3);
    private DotNetObjectReference<KanbanEventService>? _objRef;
    private IJSObjectReference? _module;
    private readonly Random _random = new(DateTime.Now.Microsecond);

    private const string LocalStorageKey = "kanban_events";
    private const string ModulePath = "/_content/CnGalWebSite.MainSite.Shared/scripts/cg-kanban-live2d.js";

    public event Action<KanbanDialogBoxModel>? OnDialogTriggered;

    public KanbanEventService(
        ILocalStorageService localStorage,
        IKanbanRemoteConfigService remoteConfig,
        IJSRuntime jsRuntime,
        NavigationManager navigation,
        ILogger<KanbanEventService> logger)
    {
        _localStorage = localStorage;
        _remoteConfig = remoteConfig;
        _jsRuntime = jsRuntime;
        _navigation = navigation;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        await LoadAsync();
        _navigation.LocationChanged += OnLocationChanged;
        await InitMouseOverEvent();
        await Task.Delay(2000, ct);
        await RunEventLoopAsync(ct);
    }

    public void NotifyUserInteraction()
    {
        _lastInteractionTime = DateTime.Now;
    }

    [JSInvokable]
    public async Task TriggerMouseOverEventAsync(int index)
    {
        if (_eventGroup is null || index < 0 || index >= _eventGroup.MouseOverEvents.Count)
        {
            return;
        }

        var item = _eventGroup.MouseOverEvents[index];
        await TriggerEventAsync(item, EventType.MouseOver, index);
    }

    public async Task TriggerCustomEventAsync(string name)
    {
        if (_eventGroup is null)
        {
            return;
        }

        var item = _eventGroup.CustomEvents.FirstOrDefault(s =>
            string.Equals(s.Name, name, StringComparison.Ordinal));
        if (item is not null)
        {
            await TriggerEventAsync(item, EventType.Custom, name);
        }
    }

    public async Task SaveAsync()
    {
        try
        {
            if (_eventGroup is null)
            {
                return;
            }

            SyncTrackingFromEventGroup();
            await _localStorage.SetItemAsync(LocalStorageKey, _tracking);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存事件追踪数据失败");
        }
    }

    public async ValueTask DisposeAsync()
    {
        _navigation.LocationChanged -= OnLocationChanged;
        _objRef?.Dispose();
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }

    private async Task LoadAsync()
    {
        try
        {
            _eventGroup = await _remoteConfig.GetEventGroupAsync();
            if (_eventGroup is null)
            {
                _eventGroup = new KanbanEventGroupModel();
            }

            var localTracking = await _localStorage.GetItemAsync<KanbanEventTrackingState>(LocalStorageKey);
            if (localTracking is not null)
            {
                _tracking = localTracking;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载事件数据失败");
            _eventGroup ??= new KanbanEventGroupModel();
        }
    }

    private async Task InitMouseOverEvent()
    {
        try
        {
            _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath);
            _objRef = DotNetObjectReference.Create(this);

            var selectors = (_eventGroup?.MouseOverEvents ?? [])
                .Select((s, i) => new { s.Selector, Index = i })
                .ToList();

            await _module.InvokeVoidAsync("initMouseOverEvent", _objRef, selectors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初始化鼠标悬停事件失败");
        }
    }

    private async Task RunEventLoopAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
        try
        {
            while (await timer.WaitForNextTickAsync(ct))
            {
                ProcessTick();
                DrainDialogQueue();
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // 正常取消
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "事件循环发生异常");
        }
    }

    private void ProcessTick()
    {
        if (_eventGroup is null)
        {
            return;
        }

        var now = DateTime.Now;
        var timeNow = TimeOnly.FromDateTime(now);
        var today = DateOnly.FromDateTime(now);

        ProcessTimeEvents(now, timeNow);
        ProcessDateEvents(now, today);
        ProcessIdleEvents(now);
    }

    private void ProcessTimeEvents(DateTime now, TimeOnly timeNow)
    {
        var events = _eventGroup?.TimeEvents;
        if (events is null)
        {
            return;
        }

        for (var i = 0; i < events.Count; i++)
        {
            var evt = events[i];
            var lastTime = GetTrackingTime(EventType.Time, i);
            var alreadyTriggeredToday =
                lastTime.Year == now.Year &&
                lastTime.Month == now.Month &&
                lastTime.Day == now.Day;

            if (!alreadyTriggeredToday && timeNow >= evt.AfterTime && timeNow <= evt.BeforeTime)
            {
                SetTrackingTime(EventType.Time, i, now);
                EnqueueDialog(evt);
            }
        }
    }

    private void ProcessDateEvents(DateTime now, DateOnly today)
    {
        var events = _eventGroup?.DateEvents;
        if (events is null)
        {
            return;
        }

        for (var i = 0; i < events.Count; i++)
        {
            var evt = events[i];
            var lastTime = GetTrackingTime(EventType.Date, i);
            var alreadyTriggeredToday =
                lastTime.Year == now.Year &&
                lastTime.Month == now.Month &&
                lastTime.Day == now.Day;

            if (!alreadyTriggeredToday && evt.Date.Month == today.Month && evt.Date.Day == today.Day)
            {
                SetTrackingTime(EventType.Date, i, now);
                EnqueueDialog(evt);
            }
        }
    }

    private void ProcessIdleEvents(DateTime now)
    {
        var events = _eventGroup?.IdleEvents;
        if (events is null || events.Count == 0)
        {
            return;
        }

        var sinceLastCheck = now - _lastIdleCheckTime;
        if (sinceLastCheck.TotalSeconds < 10)
        {
            return;
        }

        _lastIdleCheckTime = now;
        var idleDuration = now - _lastInteractionTime;
        if (idleDuration.TotalSeconds < 30)
        {
            return;
        }

        if (_random.Next(0, 10000) < 10)
        {
            var idx = _random.Next(0, events.Count);
            EnqueueDialog(events[idx]);
        }
    }

    private void EnqueueDialog(KanbanEventModel eventModel)
    {
        var dialog = BuildDialogModel(eventModel);
        if (dialog is null)
        {
            return;
        }

        if (dialog.Priority > 0)
        {
            _dialogQueue.Clear();
        }

        if (_dialogQueue.Count >= 3)
        {
            return;
        }

        _dialogQueue.Add(dialog);
    }

    private void DrainDialogQueue()
    {
        while (_dialogQueue.Count > 0)
        {
            var dialog = _dialogQueue[0];
            _dialogQueue.RemoveAt(0);
            OnDialogTriggered?.Invoke(dialog);
        }
    }

    private static KanbanDialogBoxModel? BuildDialogModel(KanbanEventModel eventModel)
    {
        if (eventModel.Contents.Count == 0)
        {
            return null;
        }

        var contentIndex = Random.Shared.Next(0, eventModel.Contents.Count);
        return new KanbanDialogBoxModel
        {
            Content = eventModel.Contents[contentIndex],
            Expression = eventModel.Expression,
            MotionGroup = eventModel.MotionGroup,
            Motion = eventModel.Motion,
            Priority = eventModel.Priority,
            Type = eventModel.DialogType
        };
    }

    private async Task TriggerEventAsync(KanbanEventModel eventModel, EventType type, object key)
    {
        SetTrackingTime(type, key, DateTime.Now);
        EnqueueDialog(eventModel);
        await SaveAsync();
    }

    private void OnLocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
    {
        if (_eventGroup is null)
        {
            return;
        }

        var url = _navigation.Uri.Replace(_navigation.BaseUri, "");
        var item = _eventGroup.NavigationEvents
            .FirstOrDefault(s => string.Equals(s.Url, url, StringComparison.Ordinal));
        if (item is not null)
        {
            _ = TriggerNavigationEventAsync(item, url);
        }
    }

    private async Task TriggerNavigationEventAsync(KanbanNavigationEventModel item, string url)
    {
        try
        {
            await TriggerEventAsync(item, EventType.Navigation, url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "触发导航事件失败 Url={Url}", url);
        }
    }

    private DateTime GetTrackingTime(EventType type, object key)
    {
        return type switch
        {
            EventType.Time => GetListTrackingTime(_tracking.TimeEventTriggerTimes, (int)key),
            EventType.Date => GetListTrackingTime(_tracking.DateEventTriggerTimes, (int)key),
            EventType.Idle => GetListTrackingTime(_tracking.IdleEventTriggerTimes, (int)key),
            EventType.MouseOver => GetListTrackingTime(_tracking.MouseOverEventTriggerTimes, (int)key),
            EventType.Custom => _tracking.CustomEventTriggerTimes.TryGetValue((string)key, out var dt) ? dt : DateTime.MinValue,
            EventType.Navigation => _tracking.NavigationEventTriggerTimes.TryGetValue((string)key, out var dt) ? dt : DateTime.MinValue,
            _ => DateTime.MinValue
        };
    }

    private static DateTime GetListTrackingTime(List<DateTime> list, int index)
    {
        if (index < 0 || index >= list.Count)
        {
            return DateTime.MinValue;
        }

        return list[index];
    }

    private void SetTrackingTime(EventType type, object key, DateTime time)
    {
        switch (type)
        {
            case EventType.Time:
                SetListTrackingTime(_tracking.TimeEventTriggerTimes, (int)key, time);
                break;
            case EventType.Date:
                SetListTrackingTime(_tracking.DateEventTriggerTimes, (int)key, time);
                break;
            case EventType.Idle:
                SetListTrackingTime(_tracking.IdleEventTriggerTimes, (int)key, time);
                break;
            case EventType.MouseOver:
                SetListTrackingTime(_tracking.MouseOverEventTriggerTimes, (int)key, time);
                break;
            case EventType.Custom:
                _tracking.CustomEventTriggerTimes[(string)key] = time;
                break;
            case EventType.Navigation:
                _tracking.NavigationEventTriggerTimes[(string)key] = time;
                break;
        }
    }

    private static void SetListTrackingTime(List<DateTime> list, int index, DateTime time)
    {
        while (list.Count <= index)
        {
            list.Add(DateTime.MinValue);
        }

        list[index] = time;
    }

    private void SyncTrackingFromEventGroup()
    {
        if (_eventGroup is null)
        {
            return;
        }

        PadList(_tracking.TimeEventTriggerTimes, _eventGroup.TimeEvents.Count);
        PadList(_tracking.DateEventTriggerTimes, _eventGroup.DateEvents.Count);
        PadList(_tracking.IdleEventTriggerTimes, _eventGroup.IdleEvents.Count);
        PadList(_tracking.MouseOverEventTriggerTimes, _eventGroup.MouseOverEvents.Count);
    }

    private static void PadList(List<DateTime> list, int targetCount)
    {
        while (list.Count < targetCount)
        {
            list.Add(DateTime.MinValue);
        }
    }

    private enum EventType
    {
        Time,
        Date,
        Idle,
        MouseOver,
        Custom,
        Navigation
    }

    private sealed class KanbanEventTrackingState
    {
        public List<DateTime> TimeEventTriggerTimes { get; set; } = new();
        public List<DateTime> DateEventTriggerTimes { get; set; } = new();
        public List<DateTime> IdleEventTriggerTimes { get; set; } = new();
        public List<DateTime> MouseOverEventTriggerTimes { get; set; } = new();
        public Dictionary<string, DateTime> CustomEventTriggerTimes { get; set; } = new();
        public Dictionary<string, DateTime> NavigationEventTriggerTimes { get; set; } = new();
    }
}
