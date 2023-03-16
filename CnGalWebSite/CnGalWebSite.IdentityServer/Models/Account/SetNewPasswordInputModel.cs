using IdentityServer4.Models;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.IdentityServer.Models.Account
{
    public class SetNewPasswordInputModel
    {
        [Required(ErrorMessage = "请输入密码")]
        [DataType(DataType.Password)]
        [Display(Name = "密码")]
        public string Password { get; set; }

        [Required(ErrorMessage = "请输入密码")]
        [Display(Name = "确认密码")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "两次输入的密码不一致，请重新输入")]
        public string ConfirmPassword { get; set; }

        public string UserId { get; set; }
        public string Token { get; set; }

        public string ReturnUrl { get; set; }
    }
}
