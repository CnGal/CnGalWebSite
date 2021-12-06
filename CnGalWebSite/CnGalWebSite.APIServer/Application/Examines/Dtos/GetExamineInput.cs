using CnGalWebSite.DataModel.Application.Dtos;

namespace CnGalWebSite.APIServer.ExamineX
{
    public class GetExamineInput : PagedSortedAndFilterInput
    {
        public bool IsVisual { get; set; }

        public string UserId { get; set; }

        public GetExamineInput()
        {
            Sorting = "Id";
            ScreeningConditions = "待审核";
        }
    }
}
