using CnGalWebSite.DataModel.Sorting;


namespace CnGalWebSite.APIServer.Application.Comments.Dtos
{
    public class GetCommentInput : PagedSortedAndFilterInput<CommentSortField>
    {
        public GetCommentInput()
        {
            Sorts = [new(CommentSortField.Id, CnGalWebSite.Core.Models.SortDirection.Descending)];
            ScreeningConditions = "全部";
        }
    }
}
