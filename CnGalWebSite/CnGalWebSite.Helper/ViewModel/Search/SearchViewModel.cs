using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.ViewModel.Search;
namespace CnGalWebSite.DataModel.ViewModel.Home
{
    public class SearchViewModel
    {
        //在这里把每个分页都储存起来 使用另一个包装的分页
        public PagedResultDto<SearchAloneModel> pagedResultDto { get; set; } = new PagedResultDto<SearchAloneModel>();
    }


}
