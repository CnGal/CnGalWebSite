using CnGalWebSite.DataModel.Application.Dtos;

namespace CnGalWebSite.DataModel.Application.Roles.Dtos
{
    public class GetRoleInput : PagedSortedAndFilterInput
    {
        public GetRoleInput()
        {
            Sorting = "Id";
            ScreeningConditions = "全部";
        }
    }

}
