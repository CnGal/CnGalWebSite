using CnGalWebSite.DataModel.Application.Dtos;

namespace CnGalWebSite.DataModel.Application.Articles.Dtos
{
    public class GetArticleInput : PagedSortedAndFilterInput
    {
        public GetArticleInput()
        {
            Sorting = "Id";
            ScreeningConditions = "全部";
        }
    }
}
