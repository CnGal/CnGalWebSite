using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ExamineModel
{
    public class EntryImages
    {
        public List<EntryImage> Images { get; set; }=new List<EntryImage>();
    }

    public class EntryImage
    {
        public string Url { get; set; }

        public string Note { get; set; }

        public string Modifier { get; set; }

        public bool IsDelete { get; set; }
    }
}
