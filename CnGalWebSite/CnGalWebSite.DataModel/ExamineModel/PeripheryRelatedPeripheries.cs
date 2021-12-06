using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ExamineModel
{
    public class PeripheryRelatedPeripheries
    {
        public List<PeripheryRelatedPeripheriesAloneModel> Relevances { get; set; } = new List<PeripheryRelatedPeripheriesAloneModel>();

    }
    public class PeripheryRelatedPeripheriesAloneModel
    {
        public bool IsDelete { get; set; }

        public long PeripheryId { get; set; }
    }
}
