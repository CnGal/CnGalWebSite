using CnGalWebSite.DataModel.Sorting;


namespace CnGalWebSite.APIServer.Application.Articles.Dtos
{
    public class GetArticleInput : PagedSortedAndFilterInput<ArticleSortField>
    {
        public GetArticleInput()
        {
            Sorts = [new(ArticleSortField.Id, CnGalWebSite.Core.Models.SortDirection.Ascending)];
            ScreeningConditions = "全部";
        }
    }
}
