using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.News
{
   public  class AddWeiboNewsModel
    {
        [Display(Name ="微博链接")]
        public string Link { get; set; }
    }
}
