using CnGalWebSite.DataModel.Application.Dtos;
using CnGalWebSite.DataModel.ViewModel.Search;
namespace CnGalWebSite.DataModel.ViewModel.Home
{
    public class SearchViewModel
    {
        //在这里把每个分页都储存起来 使用另一个包装的分页
        public PagedResultDto<SearchAloneModel> pagedResultDto { get; set; }
    }

    /// <summary>
    /// 这个作为数据传递给前端 前端判断到底什么有值再显示对应格式
    /// </summary>
    public class SearchAloneModel
    {
        public EntryInforTipViewModel entry { get; set; }

        public ArticleInforTipViewModel article { get; set; }

        public UserInforTipViewModel user { get; set; }

        public TagInforTipViewModel tag { get; set; }

        public PeripheryInforTipViewModel periphery { get; set; }
    }
}
