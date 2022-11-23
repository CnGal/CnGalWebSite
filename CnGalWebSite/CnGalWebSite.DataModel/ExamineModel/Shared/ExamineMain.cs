using System.Collections.Generic;

namespace CnGalWebSite.DataModel.ExamineModel.Shared
{
    public class ExamineMain
    {
        public List<ExamineMainAlone> Items { get; set; } = new List<ExamineMainAlone>();
    }
    public class ExamineMainAlone
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

}
