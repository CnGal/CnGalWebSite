using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Coments;
using CnGalWebSite.Helper.ViewModel.Comments;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Infrastructure;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Models.Comments;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Queries;

/// <summary>
/// 评论查询服务实现。
/// </summary>
public sealed class CommentQueryService(HttpClient httpClient, ILogger<CommentQueryService> logger)
    : QueryServiceBase(httpClient), ICommentQueryService
{
    protected override ILogger Logger { get; } = logger;

    public Task<SdkResult<CommentListViewModel>> GetCommentsAsync(
        CommentType type,
        string objectId,
        CancellationToken cancellationToken = default)
    {
        var path = $"api/comments/GetComments/{(int)type}/{objectId}";
        return GetSingleAsync<CommentCacheModel, CommentListViewModel>(
            path,
            MapToViewModel,
            "COMMENT",
            "评论列表",
            $"{type}/{objectId}",
            cancellationToken);
    }

    private static CommentListViewModel MapToViewModel(CommentCacheModel dto)
    {
        return new CommentListViewModel
        {
            Items = dto.Items?.Select(MapComment).ToList() ?? [],
            TotalCount = dto.Items?.Count ?? 0
        };
    }

    private static CommentItemViewModel MapComment(CommentViewModel comment)
    {
        return new CommentItemViewModel
        {
            Id = comment.Id,
            Text = comment.Text ?? "",
            CommentTime = comment.CommentTime,
            User = new CommentUserViewModel
            {
                Id = comment.UserInfor?.Id ?? "",
                Name = comment.UserInfor?.Name ?? "",
                PhotoPath = comment.UserInfor?.PhotoPath ?? ""
            },
            Children = comment.InverseParentCodeNavigation?
                .Select(MapComment)
                .ToList() ?? []
        };
    }
}
