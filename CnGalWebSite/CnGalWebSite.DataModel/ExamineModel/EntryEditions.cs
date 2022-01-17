using CnGalWebSite.DataModel.Model;
using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ExamineModel
{
    public class EntryEditions
    {
        public List<EditionModel_> Editions { get; set; }
    }

    public class EditionModel_
    {
        public bool IsDelete { get; set; }

        public EditionModel Data { get; set; }
    }
}
