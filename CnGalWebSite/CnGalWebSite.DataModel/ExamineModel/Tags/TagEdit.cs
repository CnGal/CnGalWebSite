using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ExamineModel.Tags
{
    public class TagEdit
    {
        public string ParentTag { get; set; }
        public string Name { get; set; }

        public List<TagEditAloneModel> ChildrenTags { get; set; }
        public List<TagEditAloneModel> ChildrenEntries { get; set; }
    }
    public class TagEditAloneModel
    {
        public bool IsDelete { get; set; }
        public string Name { get; set; }
    }
}
