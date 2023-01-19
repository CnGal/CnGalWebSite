


namespace CnGalWebSite.APIServer.Application.Users.Dtos
{
    public class GetUserInput : PagedSortedAndFilterInput
    {
        public GetUserInput()
        {
            Sorting = "Id";
            ScreeningConditions = "全部";
        }
    }
}
