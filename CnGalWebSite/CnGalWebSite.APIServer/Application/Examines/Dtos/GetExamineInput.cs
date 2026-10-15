using CnGalWebSite.DataModel.Sorting;


namespace CnGalWebSite.APIServer.ExamineX
{
    public class GetExamineInput : PagedSortedAndFilterInput<ExamineSortField>
    {
        public bool IsVisual { get; set; }

        public string UserId { get; set; }

        public GetExamineInput()
        {
            Sorts = [new(ExamineSortField.Id, CnGalWebSite.Core.Models.SortDirection.Ascending)];
            ScreeningConditions = "待审核";
        }
    }
}
