using CnGalWebSite.DataModel.Application.Dtos;

namespace CnGalWebSite.DataModel.Application.Users.Dtos
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
