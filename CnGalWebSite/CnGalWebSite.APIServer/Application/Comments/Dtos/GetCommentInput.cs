

namespace CnGalWebSite.APIServer.Application.Comments.Dtos
{
    public class GetCommentInput : PagedSortedAndFilterInput
    {
        public GetCommentInput()
        {
            Sorting = "Id desc";
            ScreeningConditions = "全部";
        }
    }
}
