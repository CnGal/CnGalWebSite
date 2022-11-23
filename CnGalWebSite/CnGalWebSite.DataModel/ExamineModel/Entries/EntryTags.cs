using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ExamineModel.Entries
{
    public class EntryTags
    {
        public List<EntryTagsAloneModel> Tags { get; set; } = new List<EntryTagsAloneModel>();
    }

    public class EntryTagsAloneModel
    {
        public bool IsDelete { get; set; }

        public int TagId { get; set; }
    }
}
