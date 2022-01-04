using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.Application.Dtos
{
    public class PagedSortedAndFilterInput
    {
        public PagedSortedAndFilterInput()
        {
            CurrentPage = 1;
            MaxResultCount = 10;
            Sorting = "Id desc";
        }
        /// <summary>
        /// 每条分页数
        /// </summary>
        [Range(0, 10000)]
        public int MaxResultCount { get; set; }
        /// <summary>
        /// 当前页
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// 排序字段
        /// </summary>
        public string Sorting { get; set; }

        /// <summary>
        /// 查询名称
        /// </summary>
        public string FilterText { get; set; }

        /// <summary>
        /// 筛选条件 也可作为分类条件
        /// </summary>
        public string ScreeningConditions { get; set; }
    }
}
