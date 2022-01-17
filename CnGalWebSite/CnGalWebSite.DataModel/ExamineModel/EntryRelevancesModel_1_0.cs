using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ExamineModel
{
    public class EntryRelevancesModel_1_0
    {
        public List<EntryRelevancesExaminedModel_1_0> Relevances { get; set; }
    }
    public class EntryRelevancesExaminedModel_1_0
    {
        public bool IsDelete { get; set; }
      
        public string Modifier { get; set; }
    
        public string DisplayName { get; set; }
     
        public string DisplayValue { get; set; }
       
        public string Link { get; set; }
    }
}
