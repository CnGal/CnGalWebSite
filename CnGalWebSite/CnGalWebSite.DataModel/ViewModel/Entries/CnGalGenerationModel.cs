using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Entries
{
    public class CnGalGenerationModel
    {
        public string Name { get; set; }

        public bool Selected { get; set; }
    }

    public class CnGalGenerationYearModel
    {
        public int Year { get; set; }

        public List<CnGalGenerationModel> Games { get; set; }
    }

}
