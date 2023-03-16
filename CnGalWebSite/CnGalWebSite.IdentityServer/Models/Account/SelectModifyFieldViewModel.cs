using CnGalWebSite.IdentityServer.Models.Geetest;
using IdentityServerHost.Quickstart.UI;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CnGalWebSite.IdentityServer.Models.Account
{
    public class SelectModifyFieldViewModel: SelectModifyFieldInputModel
    {
        public bool ShowCpltToast { get; set; }
        public List<SelectModifyAccountFieldModel> AccountFields { get; set; } = new List<SelectModifyAccountFieldModel>();
        public List<SelectModifyExternalFieldModel> ExternalFields { get; set; } = new List<SelectModifyExternalFieldModel>();

        public GeetestCodeModel GeetestCode { get; set; } = new GeetestCodeModel();

    }

    public class SelectModifyExternalFieldModel : ExternalProvider
    {
        public IDictionary<SelectModifyFieldActionType, string> Actions { get; set; } = new Dictionary<SelectModifyFieldActionType, string>();
    }

    public class SelectModifyAccountFieldModel
    {
        public SelectModifyFieldType Type { get; set; }
        public IDictionary<SelectModifyFieldActionType, string> Actions { get; set; } =new Dictionary<SelectModifyFieldActionType, string>();
    }

    public enum SelectModifyFieldActionType
    {
        [Display(Name ="绑定")]
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
