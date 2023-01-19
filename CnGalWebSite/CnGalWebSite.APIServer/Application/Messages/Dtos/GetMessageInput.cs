


namespace CnGalWebSite.APIServer.Application.Users.Dtos
{
    public class GetMessageInput : PagedSortedAndFilterInput
    {
        public bool IsVisual { get; set; }

        public GetMessageInput()
        {
            Sorting = "Id desc";
            ScreeningConditions = "全部";
        }
    }
}
