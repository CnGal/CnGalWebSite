using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ExamineModel
{
    public class PeripheryRelatedEntries
    {
        public List<PeripheryRelatedEntryAloneModel> Relevances { get; set; } = new List<PeripheryRelatedEntryAloneModel>();
    }

    public class PeripheryRelatedEntryAloneModel
    {
        public bool IsDelete { get; set; }

        public string Name { get; set; }

        public int EntryId { get; set; }
    }
}
