using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Almanacs
{
    public class AlmanacOverviewModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string BriefIntroduction { get; set; }

        public int Year { get; set; }

        public long EntryCount { get; set; }

        public long ArticleCount { get; set; }

        public DateTime UpdateTime { get; set; }
    }
}
