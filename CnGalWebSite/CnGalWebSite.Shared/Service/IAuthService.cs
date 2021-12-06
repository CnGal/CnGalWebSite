using CnGalWebSite.DataModel.Model;
using System.Threading.Tasks;

namespace CnGalWebSite.Shared.Service
{
    public interface IAuthService
    {
        /// <summary>
        /// 提交registerModel给accounts controller并返回RegisterResult给调用者
        /// </summary>
        /// <param name="registerModel"></param>
        /// <returns></returns>
        Task<Result> Register(RegisterModel registerModel);

        /// <summary>
        /// 类似于Register 方法，它将LoginModel 发送给login controller
        /// 但是，当返回一个成功的结果时
        /// 它将返回一个授权令牌并持久化到local storge
        /// </summary>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        Task<LoginResult> Login(LoginModel loginModel);
        Task<bool> Login(string JwtToken);

        /// <summary>
        /// 执行与Login 方法相反的操作。
        /// </summary>
        /// <returns></returns>
        Task Logout();
        /// <summary>
        /// 注册用户时验证电子邮箱
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<ConfirmEmailRegisterResult> ConfirmEmailRegister(ConfirmEmailRegisterModel model);
        /// <summary>
        /// 类似于 Login 方法
        /// 但是，当返回一个成功的结果时
        /// 它将返回一个授权令牌并持久化到local storge
        /// </summary>
        /// <returns></returns>
        Task<LoginResult> Refresh();
        /// <summary>
        /// 检查是否已经登入
        /// </summary>
        /// <returns></returns>
        Task<bool> IsLogin();

    }
}
