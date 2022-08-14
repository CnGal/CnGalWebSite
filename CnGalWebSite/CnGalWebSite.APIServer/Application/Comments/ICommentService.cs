using BootstrapBlazor.Components;
using CnGalWebSite.APIServer.Application.Comments.Dtos;
using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Coments;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Comments
{
    public interface ICommentService
    {
        Task<List<CommentViewModel>> GetComments(CommentType type, string Id, string rankName, string ascriptionUserId, IEnumerable<Comment> examineComments);

        Task<QueryData<ListCommentAloneModel>> GetPaginatedResult(CnGalWebSite.DataModel.ViewModel.Search.QueryPageOptions options, ListCommentAloneModel searchModel, CommentType type = CommentType.None, string objectId = "");

        Task UpdateCommentData(Comment comment, Examine examine);

        Task UpdateCommentDataMainAsync(Comment comment, CommentText examine);
    }
}
