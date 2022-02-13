using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Space
{
    public class EditUserAddressModel
    {
        [Display(Name ="真实姓名")]
        public string RealName { get; set; }

        [Display(Name = "手机号码")]
        public string PhoneNumber { get; set; }

        [Display(Name = "详细地址")]
        public string Address { get; set; }
    }
}
