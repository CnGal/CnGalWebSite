using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.Model
{
    public class ExpoTag
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public int Priority { get; set; }

        public bool Hidden { get; set; }

        public List<ExpoGame> Games { get; set; }
    }
}
