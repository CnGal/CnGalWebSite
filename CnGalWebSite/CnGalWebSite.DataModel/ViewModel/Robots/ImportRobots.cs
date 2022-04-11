using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Robots
{
    public class ImportRobotsModel
    {
        [Display(Name ="Json字符串")]
        public string Value { get; set; }
    }
}
