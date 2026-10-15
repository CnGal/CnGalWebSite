using CnGalWebSite.DataModel.Sorting;


namespace CnGalWebSite.APIServer.Application.PlayedGames.Dtos
{
    public class GetPlayedGameInput : PagedSortedAndFilterInput<GameRecordSortField>
    {
        public GetPlayedGameInput()
        {
            Sorts = [new(GameRecordSortField.Id, CnGalWebSite.Core.Models.SortDirection.Ascending)];
            ScreeningConditions = "全部";
        }
    }
}
