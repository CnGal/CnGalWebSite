using CnGalWebSite.DataModel.Sorting;



namespace CnGalWebSite.APIServer.Application.Users.Dtos
{
    public class GetUserInput : PagedSortedAndFilterInput<UserSortField>
    {
        public GetUserInput()
        {
            Sorts = [new(UserSortField.Id, CnGalWebSite.Core.Models.SortDirection.Ascending)];
            ScreeningConditions = "全部";
        }
    }
}
