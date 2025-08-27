using CnGalWebSite.Kanban.ChatGPT.Models.UserProfile;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.ChatGPT.Services.UserProfileService
{
    /// <summary>
    /// 用户个性化设定服务接口
    /// </summary>
    public interface IUserProfileService
    {
        /// <summary>
        /// 获取用户设定
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>用户设定，如果不存在则返回默认设定</returns>
        Task<UserProfileModel> GetUserProfileAsync(string userId);

        /// <summary>
        /// 更新用户设定
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="request">更新请求</param>
        /// <returns>更新结果</returns>
        Task<bool> UpdateUserProfileAsync(string userId, UpdateUserProfileRequest request);

        /// <summary>
        /// 设置用户昵称
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="nickname">昵称</param>
        /// <returns>设置结果</returns>
        Task<bool> SetUserNicknameAsync(string userId, string nickname);

        /// <summary>
        /// 设置用户性格
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="personality">性格描述</param>
        /// <returns>设置结果</returns>
        Task<bool> SetUserPersonalityAsync(string userId, string personality);

        /// <summary>
        /// 添加用户兴趣
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="interest">兴趣</param>
        /// <returns>添加结果</returns>
        Task<bool> AddUserInterestAsync(string userId, string interest);

        /// <summary>
        /// 移除用户兴趣
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="interest">兴趣</param>
        /// <returns>移除结果</returns>
        Task<bool> RemoveUserInterestAsync(string userId, string interest);

        /// <summary>
        /// 设置用户沟通风格偏好
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="style">沟通风格</param>
        /// <returns>设置结果</returns>
        Task<bool> SetUserCommunicationStyleAsync(string userId, string style);

        /// <summary>
        /// 获取用户的个性化系统消息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="baseSystemMessage">基础系统消息</param>
        /// <returns>个性化后的系统消息</returns>
        Task<string> GetPersonalizedSystemMessageAsync(string userId, string baseSystemMessage);
    }
}
