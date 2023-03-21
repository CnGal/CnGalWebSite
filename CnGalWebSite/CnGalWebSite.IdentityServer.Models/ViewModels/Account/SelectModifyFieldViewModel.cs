using CnGalWebSite.IdentityServer.Models.DataModels.Account;
using CnGalWebSite.IdentityServer.Models.DataModels.Geetest;
using CnGalWebSite.IdentityServer.Models.InputModels.Account;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CnGalWebSite.IdentityServer.Models.ViewModels.Account
{
    public class SelectModifyFieldViewModel : SelectModifyFieldInputModel
    {
        public bool ShowCpltToast { get; set; }

        public AccountBindInfor BindInfor { get; set; } = new AccountBindInfor();
    }

    public class SelectModifyExternalFieldModel : ExternalProvider
    {
        public IDictionary<SelectModifyFieldActionType, string> Actions { get; set; } = new Dictionary<SelectModifyFieldActionType, string>();
    }

    public class SelectModifyAccountFieldModel
    {
        public SelectModifyFieldType Type { get; set; }
        public Dictionary<SelectModifyFieldActionType, string> Actions { get; set; } = new Dictionary<SelectModifyFieldActionType, string>();

        public string Icon
        {
            get
            {
                return Type switch
                {
                    SelectModifyFieldType.Password => "mdi  mdi-lock",
                    SelectModifyFieldType.Email => "mdi mdi-email",
                    SelectModifyFieldType.PhoneNumber => "mdi mdi-cellphone",
                    _ => "mdi mdi-loc",
                };
            }
        }
    }

    public enum SelectModifyFieldActionType
    {
        [Display(Name = "绑定")]
        Bind,
        [Display(Name = "修改")]
        Edit,
        [Display(Name = "解绑")]
        Unbind
    }

    public enum SelectModifyFieldType
    {
        [Display(Name = "无")]
        None,
        [Display(Name = "密码")]
        Password,
        [Display(Name = "电子邮箱")]
        Email,
        [Display(Name = "手机号")]
        PhoneNumber
    }

}
