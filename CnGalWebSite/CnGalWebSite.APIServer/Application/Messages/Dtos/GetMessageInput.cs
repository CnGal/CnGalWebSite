using CnGalWebSite.DataModel.Sorting;



namespace CnGalWebSite.APIServer.Application.Users.Dtos
{
    public class GetMessageInput : PagedSortedAndFilterInput<MessageSortField>
    {
        public bool IsVisual { get; set; }

        public GetMessageInput()
        {
            Sorts = [new(MessageSortField.Id, CnGalWebSite.Core.Models.SortDirection.Descending)];
            ScreeningConditions = "全部";
        }
    }
}
