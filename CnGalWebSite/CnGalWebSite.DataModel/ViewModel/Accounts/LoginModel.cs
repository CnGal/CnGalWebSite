using CnGalWebSite.DataModel.ViewModel.OperationRecords;
using CnGalWebSite.DataModel.ViewModel.Others;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.Model
{
    public class LoginModel
    {
        [Display(Name = "邮箱/用户名")]
        public string UserName { get; set; }
        [Display(Name = "密码")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Display(Name = "记住我")]
        public bool RememberMe { get; set; } = true;

        public bool isNeedVerification { get; set; }

        public HumanMachineVerificationResult Verification { get; set; }=new HumanMachineVerificationResult();

        public DeviceIdentificationModel Identification { get; set; } = new DeviceIdentificationModel();

    }
    public class LoginResult
    {
        public LoginResultCode Code { get; set; }
        /// <summary>
        /// 错误描述
        /// </summary>
        public string ErrorDescribe { get; set; }
        /// <summary>
        /// 成功后的令牌
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// 其他错误信息
        /// </summary>
        public string ErrorInfor { get; set; }
    }

    public enum LoginResultCode
    {
        [Display(Name = "成功")]
        OK,
        [Display(Name = "用户名或密码错误")]
        WrongUserNameOrPassword,
        [Display(Name = "未实名验证")]
        FailedRealNameValidation,
        [Display(Name = "未验证电子邮件")]
        FailedEmailValidation,
        [Display(Name = "未通过人机验证")]
        FailedRecaptchaValidation,
        [Display(Name = "历史用户")]
        HistoricalUser,
        [Display(Name = "失败次数过多")]
        FailedTooMany,
        [Display(Name = "用户被封禁")]
        UserBanded,
    }
}
