using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace CnGalWebSite.DataModel.ExamineModel.Entries
{
    public class EntryImages
    {
        public List<EditRecordImage> Images { get; set; } = new List<EditRecordImage>();
    }

    public class EditRecordImage
    {
        public string Url { get; set; }

        public string Note { get; set; }

        public string Modifier { get; set; }

        public int Priority { get; set; }

        public bool IsDelete { get; set; }
    }
}
