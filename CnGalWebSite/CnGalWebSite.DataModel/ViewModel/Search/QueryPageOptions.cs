namespace CnGalWebSite.DataModel.ViewModel.Search
{
    public class QueryPageOptions
    {
        //
        // 摘要:
        //     每页数据数量 默认 20 行
        internal const int DefaultPageItems = 20;

        //
        // 摘要:
        //     获得/设置 查询关键字
        public string SearchText { get; set; }



        //
        // 摘要:
        //     获得/设置 排序字段名称
        public string SortName { get; set; }

        //
        // 摘要:
        //     获得/设置 排序方式
        public SortOrder SortOrder
        {
            get;
            set;
        }




        //
        // 摘要:
        //     获得/设置 搜索条件绑定模型
        public object SearchModel { get; set; }

        //
        // 摘要:
        //     获得/设置 当前页码 首页为 第一页
        public int PageIndex
        {
            get;
            set;
        } = 1;


        //
        // 摘要:
        //     获得/设置 请求读取数据开始行 默认 1
        //
        // 言论：
        //     BootstrapBlazor.Components.Table`1.ScrollMode 开启虚拟滚动时使用
        public int StartIndex
        {
            get;
            set;
        }

        //
        // 摘要:
        //     获得/设置 每页条目数量
        public int PageItems
        {
            get;
            set;
        } = 20;


        //
        // 摘要:
        //     获得/设置 是否是分页查询 默认为 false
        public bool IsPage
        {
            get;
            set;
        }


    }

    public enum SortOrder
    {
        //
        // 摘要:
        //     未设置
        Unset,
        //
        // 摘要:
        //     升序 0-9 A-Z
        Asc,
        //
        // 摘要:
        //     降序 9-0 Z-A
        Desc
    }
}
