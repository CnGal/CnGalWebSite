using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.Videos;
using CnGalWebSite.MainSite.Shared.Models.Toolbox;
using CnGalWebSite.SDK.MainSite.Abstractions;

namespace CnGalWebSite.MainSite.Shared.Services.Toolbox;

public sealed class ToolboxEntryMergeService(
    IToolboxCommandService toolboxCommandService,
    IToolboxLocalRepository<MergeEntryTaskModel> mergeRepository) : ToolboxServiceBase, IToolboxEntryMergeService
{
    public async Task ProcessMergeEntryAsync(MergeEntryTaskModel model, CancellationToken cancellationToken = default)
    {
        model.Error = string.Empty;
        Report(model, ToolboxOutputLevel.Info, $"开始将<{model.SubName}>合并到<{model.HostName}>");

        if (string.IsNullOrWhiteSpace(model.HostName) || string.IsNullOrWhiteSpace(model.SubName))
        {
            Report(model, ToolboxOutputLevel.Error, "词条名称不能为空");
            return;
        }

        var history = (await mergeRepository.GetAllAsync(cancellationToken))
            .FirstOrDefault(s => s.HostName == model.HostName && s.SubName == model.SubName);
        if (history is not null && history.PostTime.HasValue)
        {
            Report(model, ToolboxOutputLevel.Error, $"{model.SubName}({history.SubId}) -> {model.HostName}({history.HostId}) 已于 {history.PostTime} 提交审核");
            return;
        }

        model.CompleteTaskCount++;
        Report(model, ToolboxOutputLevel.Info, "获取词条");
        if (!await LoadEntryIdsAsync(model, cancellationToken))
        {
            return;
        }

        model.CompleteTaskCount++;
        Report(model, ToolboxOutputLevel.Info, "生成审核记录");
        var generated = await GenerateMergePayloadsAsync(model, cancellationToken);
        if (generated is null)
        {
            return;
        }

        await mergeRepository.InsertAsync(model, cancellationToken);
        model.CompleteTaskCount++;

        Report(model, ToolboxOutputLevel.Info, "发送审核记录");
        if (!await SubmitPayloadsAsync(model, generated, cancellationToken))
        {
            return;
        }

        model.CompleteTaskCount++;
        Report(model, ToolboxOutputLevel.Info, "隐藏从词条");
        if (!await HideSubEntryAsync(model, cancellationToken))
        {
            return;
        }

        model.CompleteTaskCount++;
        model.PostTime = DateTime.Now;
        await mergeRepository.SaveAsync(cancellationToken);
        model.CompleteTaskCount++;

        Report(model, ToolboxOutputLevel.Info, $"成功将 {model.SubName} 合并到 {model.HostName}");
    }

    private async Task<bool> LoadEntryIdsAsync(MergeEntryTaskModel model, CancellationToken cancellationToken)
    {
        var hostIdResult = await toolboxCommandService.GetEntryIdAsync(model.HostName, cancellationToken);
        var subIdResult = await toolboxCommandService.GetEntryIdAsync(model.SubName, cancellationToken);
        if (!hostIdResult.Success || !subIdResult.Success)
        {
            Report(model, ToolboxOutputLevel.Error, hostIdResult.Error?.Message ?? subIdResult.Error?.Message ?? "获取词条失败");
            return false;
        }

        model.HostId = hostIdResult.Data;
        model.SubId = subIdResult.Data;
        return true;
    }

    private async Task<MergePayloads?> GenerateMergePayloadsAsync(MergeEntryTaskModel model, CancellationToken cancellationToken)
    {
        var hostResult = await toolboxCommandService.GetEntryViewAsync(model.HostId, cancellationToken);
        var subResult = await toolboxCommandService.GetEntryViewAsync(model.SubId, cancellationToken);
        if (!hostResult.Success || hostResult.Data is null || !subResult.Success || subResult.Data is null)
        {
            Report(model, ToolboxOutputLevel.Error, hostResult.Error?.Message ?? subResult.Error?.Message ?? "无法获取目标词条");
            return null;
        }

        var hostEntry = hostResult.Data;
        var subEntry = subResult.Data;
        if (subEntry.Type == EntryType.Game || hostEntry.Type == EntryType.Game)
        {
            Report(model, ToolboxOutputLevel.Error, "目前不支持合并游戏");
            return null;
        }

        var payloads = new MergePayloads();
        var reEntryIds = subEntry.Roles.Select(x => x.Id)
            .Concat(subEntry.ProductionGroups.Select(x => x.Id))
            .Concat(subEntry.EntryRelevances.Select(x => x.Id))
            .Concat(subEntry.StaffGames.Select(x => x.Id))
            .Concat(hostEntry.Roles.Select(x => x.Id))
            .Concat(hostEntry.ProductionGroups.Select(x => x.Id))
            .Concat(hostEntry.EntryRelevances.Select(x => x.Id))
            .Concat(hostEntry.StaffGames.Select(x => x.Id))
            .Distinct()
            .ToList();

        foreach (var id in reEntryIds)
        {
            var result = await toolboxCommandService.GetEntryRelevancesAsync(id, cancellationToken);
            if (!result.Success || result.Data is null)
            {
                Report(model, ToolboxOutputLevel.Error, result.Error?.Message ?? $"获取词条关联编辑数据失败：{id}");
                return null;
            }

            ReplaceEntryName(result.Data.Games, model.SubName, model.HostName);
            ReplaceEntryName(result.Data.staffs, model.SubName, model.HostName);
            ReplaceEntryName(result.Data.Roles, model.SubName, model.HostName);
            ReplaceEntryName(result.Data.Groups, model.SubName, model.HostName);
            payloads.EntryRelevances.Add(result.Data);
        }

        model.CompleteTaskCount++;

        var reGameIds = subEntry.EntryRelevances.Where(s => s.Type == EntryType.Game || s.Type == EntryType.Role).Select(x => x.Id)
            .Concat(subEntry.StaffGames.Select(x => x.Id))
            .Concat(subEntry.Roles.Select(x => x.Id))
            .Concat(hostEntry.EntryRelevances.Where(s => s.Type == EntryType.Game || s.Type == EntryType.Role).Select(x => x.Id))
            .Concat(hostEntry.StaffGames.Select(x => x.Id))
            .Concat(hostEntry.Roles.Select(x => x.Id))
            .Distinct()
            .ToList();

        foreach (var id in reGameIds)
        {
            var result = await toolboxCommandService.GetEntryAddInforAsync(id, cancellationToken);
            if (!result.Success || result.Data is null)
            {
                Report(model, ToolboxOutputLevel.Error, result.Error?.Message ?? $"获取词条附加信息编辑数据失败：{id}");
                return null;
            }

            foreach (var staff in result.Data.Staffs.Where(s => s.Name == model.SubName))
            {
                staff.Name = model.HostName;
            }

            result.Data.Publisher = ReplaceEntryName(result.Data.Publisher, model.SubName, model.HostName);
            result.Data.ProductionGroup = ReplaceEntryName(result.Data.ProductionGroup, model.SubName, model.HostName);
            result.Data.CV = ReplaceEntryName(result.Data.CV, model.SubName, model.HostName);
            payloads.EntryAddInfors.Add(result.Data);
        }

        model.CompleteTaskCount++;

        var articleIds = subEntry.ArticleRelevances.Select(x => x.Id)
            .Concat(subEntry.NewsOfEntry.Select(x => x.ArticleId))
            .Distinct()
            .ToList();
        foreach (var id in articleIds)
        {
            var result = await toolboxCommandService.GetArticleRelevancesAsync(id, cancellationToken);
            if (!result.Success || result.Data is null)
            {
                Report(model, ToolboxOutputLevel.Error, result.Error?.Message ?? $"获取文章关联编辑数据失败：{id}");
                return null;
            }

            ReplaceEntryName(result.Data.Games, model.SubName, model.HostName);
            ReplaceEntryName(result.Data.Staffs, model.SubName, model.HostName);
            ReplaceEntryName(result.Data.Roles, model.SubName, model.HostName);
            ReplaceEntryName(result.Data.Groups, model.SubName, model.HostName);
            payloads.ArticleRelevances.Add(result.Data);
        }

        var videoIds = subEntry.VideoRelevances.Select(x => x.Id).Distinct().ToList();
        foreach (var id in videoIds)
        {
            var result = await toolboxCommandService.GetVideoRelevancesAsync(id, cancellationToken);
            if (!result.Success || result.Data is null)
            {
                Report(model, ToolboxOutputLevel.Error, result.Error?.Message ?? $"获取视频关联编辑数据失败：{id}");
                return null;
            }

            ReplaceEntryName(result.Data.Games, model.SubName, model.HostName);
            ReplaceEntryName(result.Data.staffs, model.SubName, model.HostName);
            ReplaceEntryName(result.Data.Roles, model.SubName, model.HostName);
            ReplaceEntryName(result.Data.Groups, model.SubName, model.HostName);
            payloads.VideoRelevances.Add(result.Data);
        }

        model.CompleteTaskCount++;
        return payloads;
    }

    private async Task<bool> SubmitPayloadsAsync(MergeEntryTaskModel model, MergePayloads payloads, CancellationToken cancellationToken)
    {
        foreach (var item in payloads.ArticleRelevances)
        {
            if (!await SubmitAsync(model, toolboxCommandService.SubmitArticleRelevancesAsync(item, cancellationToken)))
            {
                return false;
            }
        }

        foreach (var item in payloads.EntryAddInfors)
        {
            if (!await SubmitAsync(model, toolboxCommandService.SubmitEntryAddInforAsync(item, cancellationToken)))
            {
                return false;
            }
        }

        foreach (var item in payloads.EntryRelevances)
        {
            if (!await SubmitAsync(model, toolboxCommandService.SubmitEntryRelevancesAsync(item, cancellationToken)))
            {
                return false;
            }
        }

        foreach (var item in payloads.VideoRelevances)
        {
            if (!await SubmitAsync(model, toolboxCommandService.SubmitVideoRelevancesAsync(item, cancellationToken)))
            {
                return false;
            }
        }

        return true;
    }

    private async Task<bool> SubmitAsync(MergeEntryTaskModel model, Task<CnGalWebSite.SDK.MainSite.Models.SdkResult<bool>> task)
    {
        var result = await task;
        if (result.Success)
        {
            return true;
        }

        Report(model, ToolboxOutputLevel.Error, result.Error?.Message ?? "发送审核记录失败");
        return false;
    }

    private async Task<bool> HideSubEntryAsync(MergeEntryTaskModel model, CancellationToken cancellationToken)
    {
        var hideResult = await toolboxCommandService.HideEntryAsync(model.SubId, cancellationToken);
        if (hideResult.Success)
        {
            return true;
        }

        var mainResult = await toolboxCommandService.GetEntryMainAsync(model.SubId, cancellationToken);
        if (!mainResult.Success || mainResult.Data is null)
        {
            Report(model, ToolboxOutputLevel.Error, hideResult.Error?.Message ?? "递交隐藏从词条申请失败");
            return false;
        }

        mainResult.Data.Name = mainResult.Data.DisplayName = "已删除_" + mainResult.Data.Name;
        var submitResult = await toolboxCommandService.SubmitEntryMainAsync(mainResult.Data, cancellationToken);
        if (!submitResult.Success)
        {
            Report(model, ToolboxOutputLevel.Error, submitResult.Error?.Message ?? "递交隐藏从词条申请失败");
            return false;
        }

        Report(model, ToolboxOutputLevel.Warning, "已提交编辑记录，等待审核通过");
        return true;
    }

    private static void ReplaceEntryName(List<RelevancesModel> list, string oldName, string newName)
    {
        var target = list.FirstOrDefault(s => s.DisplayName == oldName);
        if (target is not null)
        {
            target.DisplayName = newName;
        }
    }

    private static string? ReplaceEntryName(string? text, string oldName, string newName)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return text;
        }

        var values = text.Replace("，", ",").Replace("、", ",").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return string.Join(',', values.Select(s => s == oldName ? newName : s));
    }

    private sealed class MergePayloads
    {
        public List<EditArticleRelevancesViewModel> ArticleRelevances { get; } = [];
        public List<EditAddInforViewModel> EntryAddInfors { get; } = [];
        public List<EditRelevancesViewModel> EntryRelevances { get; } = [];
        public List<EditVideoRelevancesViewModel> VideoRelevances { get; } = [];
    }
}
