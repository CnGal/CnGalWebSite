using System.ComponentModel.DataAnnotations;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Accounts
{
    public class GeetestCodeModel
    {
        [Display(Name = "Gt")]
        public string Gt { get; set; }
        [Display(Name = "Challenge")]
        public string Challenge { get; set; }
        [Display(Name = "Success")]
        public string Success { get; set; }
    }
}
