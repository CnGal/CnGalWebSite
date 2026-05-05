using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CnGalWebSite.MainSite.Shared.Services.KanbanModels;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Models.Kanban;

namespace CnGalWebSite.MainSite.Shared.Services;

public sealed class KanbanChatService : IKanbanChatService
{
    private readonly List<KanbanChatModel> _messages = [];
    private readonly IKanbanQueryService _kanbanQueryService;
    private readonly IKanbanLive2DService _live2DService;
    private readonly ICgDeviceIdentificationService _deviceIdentificationService;

    public IReadOnlyList<KanbanChatModel> Messages
    {
        get
        {
            FilterExpiredMessages();
            return _messages;
        }
    }

    public bool IsOpen { get; private set; }

    public KanbanChatService(
        IKanbanQueryService kanbanQueryService,
        IKanbanLive2DService live2DService,
        ICgDeviceIdentificationService deviceIdentificationService)
    {
        _kanbanQueryService = kanbanQueryService;
        _live2DService = live2DService;
        _deviceIdentificationService = deviceIdentificationService;
    }

    public async Task SendMessageAsync(string message, bool isFirst)
    {
        // 添加用户消息
        _messages.Add(new KanbanChatModel
        {
            Text = message,
            Time = DateTime.Now,
            IsUser = true,
        });

        // 生成看板娘头像
        var avatarUrl = await _live2DService.GetKanbanImageGeneration();

        // 获取设备识别信息
        var identification = await _deviceIdentificationService.GetIdentificationAsync();

        // 通过 SDK 发送请求
        var request = new KanbanChatRequest
        {
            Message = message,
            IsFirst = isFirst,
            DeviceIdentification = identification.Cookie ?? string.Empty,
        };

        var result = await _kanbanQueryService.GetChatReplyAsync(request);

        // 处理回复
        string replyText;
        if (result.Success && result.Data is not null)
        {
            replyText = result.Data.Error ?? string.Empty;
        }
        else
        {
            replyText = result.Error?.Message ?? "请求回复时发生异常";
        }

        _messages.Add(new KanbanChatModel
        {
            Image = avatarUrl,
            Text = replyText,
            Time = DateTime.Now,
            IsUser = false,
        });
    }

    public void Show()
    {
        IsOpen = true;
    }

    public void Close()
    {
        IsOpen = false;
    }

    public void Clear()
    {
        _messages.Clear();
    }

    private void FilterExpiredMessages()
    {
        var cutoff = DateTime.Now.AddHours(-24);
        _messages.RemoveAll(m => m.Time < cutoff);
    }
}
