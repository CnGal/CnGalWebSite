using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.HistoryData
{
    public class ZhiHuArticleModel
    {
        public string Title { get; set; }

        public string Url { get; set; }

        public string MainPage { get; set; }

        public string Image { get; set; }

        public bool IsCutImage { get; set; }

        public DateTime PublishTime { get; set; }
    }
}
