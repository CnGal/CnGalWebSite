

namespace CnGalWebSite.APIServer.Application.PlayedGames.Dtos
{
    public class GetPlayedGameInput : PagedSortedAndFilterInput
    {
        public GetPlayedGameInput()
        {
            Sorting = "Id";
            ScreeningConditions = "全部";
        }
    }
}
