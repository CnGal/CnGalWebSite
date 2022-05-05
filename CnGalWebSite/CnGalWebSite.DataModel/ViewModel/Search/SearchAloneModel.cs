namespace CnGalWebSite.DataModel.ViewModel.Search
{
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
