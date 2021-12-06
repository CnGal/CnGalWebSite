using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.Application.Examines.Dtos;
using CnGalWebSite.DataModel.ViewModel.Admin;
using static CnGalWebSite.DataModel.Application.Examines.ExamineService;

namespace CnGalWebSite.DataModel.Application.Examines
{
    public interface IExamineService
    {
        public PagedResultDto<ExaminedNormalListModel> GetPaginatedResult(GetExamineInput input, List<ExaminedNormalListModel> examines, GetExaminePagedType type);
    }
}
