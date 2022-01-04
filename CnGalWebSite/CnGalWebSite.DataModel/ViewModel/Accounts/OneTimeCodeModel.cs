using System.ComponentModel.DataAnnotations;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Accounts
{
    public class OneTimeCodeModel
    {
        [Display(Name = "代码")]
        public string Code { get; set; }
    }
}
