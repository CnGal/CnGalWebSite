using CnGalWebSite.DataModel.Application.Dtos;

namespace CnGalWebSite.DataModel.Application.Search.Dtos
{
    public class GetSearchInput : PagedSortedAndFilterInput
    {
        public int StartIndex { get; set; } = -1;

        public GetSearchInput()
        {
            Sorting = "Id";
            ScreeningConditions = "全部";
            MaxResultCount = 4;
        }
    }
}
