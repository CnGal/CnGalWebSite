using System;
using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Space
{
    public class UserEditInforBindModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public long EditCount { get; set; }

        public DateTime? LastEditTime { get; set; }

        public long TotalFilesSpace { get; set; }
        public long UsedFilesSpace { get; set; }

        public int PassedExamineCount { get; set; }
        public int UnpassedExamineCount { get; set; }
        public int PassingExamineCount { get; set; }
    }
}
