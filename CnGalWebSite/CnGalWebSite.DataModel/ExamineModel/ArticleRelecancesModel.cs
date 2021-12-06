namespace CnGalWebSite.DataModel.ExamineModel
{
    public class ArticleRelecancesModel
    {

        public List<ArticleRelevancesExaminedModel> Relevances { get; set; }


    }
    public class ArticleRelevancesExaminedModel
    {
        public bool IsDelete { get; set; }
        public string Modifier { get; set; }
        public string DisplayName { get; set; }
        public string DisplayValue { get; set; }
        /// <summary>
        /// 当 类别 不是可识别时 使用下方链接
        /// </summary>
        public string Link { get; set; }
    }
}

