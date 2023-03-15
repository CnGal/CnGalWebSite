using IdentityServer4.Models;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.IdentityServer.Models.Account
{
    public class BindExistAccountInputModel
    {
        [Required(ErrorMessage = "请输入用户名")]
        public string Username { get; set; }
        [Required(ErrorMessage = "请输入密码")]
        public string Password { get; set; }
    }
}
