using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ExamineModel
{
    public class TagChildTags
    {
        public List<TagChildTagAloneModel> ChildTags { get; set; } = new List<TagChildTagAloneModel>();
    }


    public class TagChildTagAloneModel
    {
        public bool IsDelete { get; set; }

        public int TagId { get; set; }
    }
}
