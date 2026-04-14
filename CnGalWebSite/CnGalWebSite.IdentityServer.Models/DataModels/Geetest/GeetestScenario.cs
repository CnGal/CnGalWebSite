namespace CnGalWebSite.IdentityServer.Models.DataModels.Geetest
{
    /// <summary>
    /// 极验验证场景
    /// </summary>
    public enum GeetestScenario
    {
        /// <summary>
        /// 登录
        /// </summary>
        Login,

        /// <summary>
        /// 注册
        /// </summary>
        Register,

        /// <summary>
        /// 重置密码
        /// </summary>
        ResetPassword,

        /// <summary>
        /// 发送验证码（重发、二次验证等）
        /// </summary>
        SendCode,

        /// <summary>
        /// 修改账户信息（修改邮箱、手机号、绑定手机号）
        /// </summary>
        AccountModify
    }
}
