using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ExamineModel
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
