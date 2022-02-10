using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ExamineModel
{
    public class PeripheryRelatedPeripheries
    {
        public List<PeripheryRelatedPeripheriesAloneModel> Relevances { get; set; } = new List<PeripheryRelatedPeripheriesAloneModel>();

    }
    public class PeripheryRelatedPeripheriesAloneModel
    {
        public bool IsDelete { get; set; }

        public string Name { get; set; }

        public long PeripheryId { get; set; }
    }
}
