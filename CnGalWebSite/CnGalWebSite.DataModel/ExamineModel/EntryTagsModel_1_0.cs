using System.Collections.Generic;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ExamineModel
{
    public class EntryTagsModel_1_0
    {
        public List<TagModel> Tags = new List<TagModel>();
    }

    public class TagModel
    {
        public string Name { get; set; }

        public bool IsDelete { get; set; }
    }

}
