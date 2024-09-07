using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.EventBus.Models
{
    public class SensitiveWordsCheckModel
    {
        public List<string> Texts { get; set; } = [];
    }

    public class SensitiveWordsResultModel
    {
        public List<string> Words { get; set; } = [];
    }
}
