using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Videos;
using CnGalWebSite.MainSite.Shared.Models.Toolbox;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Models.VideoEdit;

namespace CnGalWebSite.MainSite.Shared.Services.Toolbox;

public sealed class ToolboxVideoRepostService(
    IToolboxCommandService toolboxCommandService,
    IVideoCommandService videoCommandService,
    ToolboxImageService imageService,
    IToolboxLocalRepository<RepostVideoTaskModel> videoRepository) : ToolboxServiceBase, IToolboxVideoRepostService
{
    public async Task ProcessVideoAsync(RepostVideoTaskModel model, CancellationToken cancellationToken = default)
    {
        model.Error = string.Empty;
        Report(model, ToolboxOutputLevel.Info, $"开始处理链接 {model.Url}");

        var history = (await videoRepository.GetAllAsync(cancellationToken)).FirstOrDefault(s => s.Url == model.Url);
        if (history is not null && history.PostTime.HasValue)
        {
            Report(model, ToolboxOutputLevel.Error, $"链接 {model.Url} 已于 {history.PostTime} 提交审核[Id:{history.Id}]");
            return;
        }

        var metaResult = await videoCommandService.GetVideoEditMetaOptionsAsync(cancellationToken);
        if (!metaResult.Success || metaResult.Data is null)
        {
            Report(model, ToolboxOutputLevel.Error, metaResult.Error?.Message ?? "获取候选列表失败");
            return;
        }

        if (!string.IsNullOrWhiteSpace(model.Title) && metaResult.Data.VideoItems.Any(s => s == model.Title))
        {
            Report(model, ToolboxOutputLevel.Error, $"与现有视频《{model.Title}》重名，请自定义标题");
            return;
        }

        var createModel = await BuildCreateModelAsync(model, metaResult.Data.EntryGameItems, cancellationToken);
        if (createModel is null)
        {
            return;
        }

        await videoRepository.InsertAsync(model, cancellationToken);
        Report(model, ToolboxOutputLevel.Info, "提交审核");

        var submitResult = await videoCommandService.SubmitEditAsync(new VideoEditRequest
        {
            IsCreate = true,
            Data = createModel
        }, cancellationToken);

        if (!submitResult.Success || submitResult.Data == 0)
        {
            Report(model, ToolboxOutputLevel.Error, submitResult.Error?.Message ?? "提交视频审核失败");
            return;
        }

        model.Id = submitResult.Data;
        model.PostTime = DateTime.Now;
        model.CompleteTaskCount++;
        await videoRepository.SaveAsync(cancellationToken);
        Report(model, ToolboxOutputLevel.Info, $"成功处理 {model.Url}");
    }

    private async Task<CreateVideoViewModel?> BuildCreateModelAsync(RepostVideoTaskModel item, IEnumerable<string> games, CancellationToken cancellationToken)
    {
        var bilibiliId = ExtractBilibiliId(item.Url);
        if (string.IsNullOrWhiteSpace(bilibiliId))
        {
            Report(item, ToolboxOutputLevel.Error, "获取视频 Id 失败，请检查链接");
            return null;
        }

        Report(item, ToolboxOutputLevel.Info, "获取视频内容");
        var infoResult = await toolboxCommandService.GetBilibiliVideoInfoAsync(bilibiliId, cancellationToken);
        if (!infoResult.Success || infoResult.Data is null)
        {
            Report(item, ToolboxOutputLevel.Error, infoResult.Error?.Message ?? "获取视频信息失败");
            return null;
        }

        item.CompleteTaskCount++;
        var info = infoResult.Data;
        var model = new CreateVideoViewModel();
        model.Relevances.BilibiliId = bilibiliId;
        model.Main.Type = string.IsNullOrWhiteSpace(item.Type) ? info.Type : item.Type;
        model.Main.Copyright = info.Copyright;
        model.Main.MainPicture = info.MainPicture;
        model.Main.Name = model.Main.DisplayName = string.IsNullOrWhiteSpace(item.Title) ? info.Name : item.Title;
        model.Main.PubishTime = info.PublishTime;
        model.MainPage.Context = model.Main.BriefIntroduction = info.Description.Replace(model.Main.Name, string.Empty);
        model.Main.Duration = info.Duration;
        model.Main.IsInteractive = info.IsInteractive;
        model.Main.IsCreatedByCurrentUser = item.IsCreatedByCurrentUser;
        model.Main.OriginalAuthor = info.OriginalAuthor;

        item.CompleteTaskCount++;
        Report(item, ToolboxOutputLevel.Info, "获取快照拼图");
        var imagesResult = await toolboxCommandService.GetBilibiliVideoImageImpositionAsync(bilibiliId, cancellationToken);
        if (imagesResult.Success && imagesResult.Data is not null)
        {
            item.TotalTaskCount += imagesResult.Data.Count;
            foreach (var imageUrl in imagesResult.Data)
            {
                Report(item, ToolboxOutputLevel.Info, $"获取图片 {imageUrl}");
                model.Images.Images.Add(new EditImageAloneModel
                {
                    Image = await imageService.GetImageAsync(imageUrl, cancellationToken: cancellationToken)
                });
                item.CompleteTaskCount++;
            }
        }
        else
        {
            Report(item, ToolboxOutputLevel.Warning, imagesResult.Error?.Message ?? "获取快照拼图失败，已跳过");
        }

        item.CompleteTaskCount++;
        Report(item, ToolboxOutputLevel.Info, "转存主图");
        if (!string.IsNullOrWhiteSpace(model.Main.MainPicture))
        {
            model.Main.MainPicture = await imageService.GetImageAsync(model.Main.MainPicture, 460, 215, cancellationToken);
        }

        model.Relevances.Games.AddRange(BuildGameRelevances(model.Main.Name, model.MainPage.Context, games));
        item.CompleteTaskCount++;

        return model;
    }

    private static string ExtractBilibiliId(string url)
    {
        return url.Split('?')[0]
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .LastOrDefault(s => !string.IsNullOrWhiteSpace(s)) ?? string.Empty;
    }
}
