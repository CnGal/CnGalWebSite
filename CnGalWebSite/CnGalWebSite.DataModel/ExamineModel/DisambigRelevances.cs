using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ExamineModel
{
    public class DisambigRelevances
    {
        public List<DisambigRelevancesExaminedModel> Relevances { get; set; }
    }
    public class DisambigRelevancesExaminedModel
    {
        public bool IsDelete { get; set; }

        public long EntryId { get; set; }

        public DisambigRelevanceType Type { get; set; }
    }

    public enum DisambigRelevanceType
    {
        Entry,
        Article
    }
}
