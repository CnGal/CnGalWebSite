using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.Model
{
    public class ExpoGame
    {
        public long Id { get; set; }

        public int Priority { get; set; }

        public bool Hidden { get; set; }

        public int? GameId { get; set; }
        public Entry Game { get; set; }

        public List<ExpoTag> Tags { get; set; }
    }
}
