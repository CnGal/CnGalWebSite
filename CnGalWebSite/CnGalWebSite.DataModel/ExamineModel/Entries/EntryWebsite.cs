using CnGalWebSite.DataModel.ExamineModel.Shared;
using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ExamineModel.Entries
{
    public class EntryWebsiteExamineModel
    {
        public List<EditWebsiteImage> Carousels { get; set; } = new List<EditWebsiteImage>();

        public List<EditWebsiteImage> BackgroundImages { get; set; } = new List<EditWebsiteImage>();

        public List<ExamineMainAlone> MainInfor { get; set; } = new List<ExamineMainAlone>();
    }

    public class EditWebsiteImage
    {
        public bool IsDelete { get; set; }

        public EntryWebsiteImageType Type { get; set; }

        public string Url { get; set; }

        public int Priority { get; set; }

        public string Note { get; set; }
    }
}
