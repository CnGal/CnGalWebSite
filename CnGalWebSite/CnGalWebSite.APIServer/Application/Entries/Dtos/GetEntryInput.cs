using CnGalWebSite.DataModel.Sorting;



namespace CnGalWebSite.APIServer.Application.Entries.Dtos
{
    public class GetEntryInput : PagedSortedAndFilterInput<EntrySortField>
    {
        public GetEntryInput()
        {
            Sorts = [new(EntrySortField.Id, CnGalWebSite.Core.Models.SortDirection.Ascending)];
            ScreeningConditions = "全部";
        }
    }
}
