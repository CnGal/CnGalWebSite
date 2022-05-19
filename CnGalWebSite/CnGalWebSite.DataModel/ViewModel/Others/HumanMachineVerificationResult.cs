using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Linq;

namespace CnGalWebSite.DataModel.ViewModel.Others
{
    public class HumanMachineVerificationResult
    {
        public bool Success { get; set; }
        
        public string Challenge { get; set; }
        
        public string Validate { get; set; }
        
        public string Seccode { get; set; }
    }
}
