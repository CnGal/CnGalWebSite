using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Peripheries
{
    public class EditPeripheryRelatedPeripheriesViewModel
    {
        public string Name { get; set; }
        public long Id { get; set; }

        public List<RelevancesModel> Peripheries { get; set; } = new List<RelevancesModel>();

        [Display(Name = "备注")]
        public string Note { get; set; }
    }
}
