using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Comments.Dtos;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Coments;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Comments
{
    public interface ICommentService
    {
        Task<PagedResultDto<CommentViewModel>> GetPaginatedResult(GetCommentInput input, CommentType type, string Id, string rankName, string ascriptionUserId);
        Task<QueryData<ListCommentAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListCommentAloneModel searchModel, CommentType type = CommentType.None, string objectId = "");
    }
}
