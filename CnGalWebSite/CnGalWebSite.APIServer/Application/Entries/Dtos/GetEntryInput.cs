


namespace CnGalWebSite.APIServer.Application.Entries.Dtos
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
