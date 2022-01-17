using System.Collections.Generic;

namespace CnGalWebSite.DataModel.ExamineModel
{
    public class EntryRelevances
    {
        public List<EntryRelevancesAloneModel> Relevances { get; set; } = new List<EntryRelevancesAloneModel>();
    }
    public class EntryRelevancesAloneModel
    {
        public bool IsDelete { get; set; }

        public RelevancesType Type { get; set; }
        /// <summary>
        /// 当类别为词条、文章时 该字段为Id
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// 当类别为词条、文章时 该字段为编辑时的名称，仅作为参考
        /// </summary>
        public string DisplayValue { get; set; }
        /// <summary>
        /// 当 类别 不是可识别时 使用下方链接
        /// </summary>
        public string Link { get; set; }
    }

    public enum RelevancesType
    {
        Entry,
        Article,
        Outlink
    }
}
