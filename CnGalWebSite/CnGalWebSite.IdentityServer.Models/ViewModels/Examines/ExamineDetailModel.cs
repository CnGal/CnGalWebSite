using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.IdentityServer.Models.ViewModels.Examines
{
    public class ExamineDetailModel : ExamineOverviewModel
    {
        public List<ExamineKeyVauleModel> KeyValues { get; set; } = new List<ExamineKeyVauleModel>();
        public List<ExamineImageModel> Images { get; set; } = new List<ExamineImageModel>();
    }

    public class ExamineKeyVauleModel
    {
        public string Key { get; set; }
        public string Vaule { get; set; }
    }

    public class ExamineImageModel
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
}
