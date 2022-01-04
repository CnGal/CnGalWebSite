using CnGalWebSite.DataModel.Application.Dtos;

namespace CnGalWebSite.DataModel.Application.Entries.Dtos
{
    public class GetEntryInput : PagedSortedAndFilterInput
    {
        public GetEntryInput()
        {
            Sorting = "Id";
            ScreeningConditions = "全部";
        }
    }
}
