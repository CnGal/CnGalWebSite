using CnGalWebSite.DataModel.Model;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Entries
{
    public class RevokeExamineModel
    {
        public long Id { get; set; }

        public Operation ExamineType { get; set; }
    }
}
