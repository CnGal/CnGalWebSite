using CnGalWebSite.DataModel.Application.Dtos;

namespace CnGalWebSite.DataModel.Application.Examines.Dtos
{
    public class GetExamineInput : PagedSortedAndFilterInput
    {
        public bool IsVisual { get; set; }

        public string UserId { get; set; }


        public GetExamineInput()
        {
            Sorting = "Id desc";
            ScreeningConditions = "全部";
        }
    }
}
