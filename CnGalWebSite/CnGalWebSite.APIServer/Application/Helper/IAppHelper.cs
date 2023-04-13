using CnGalWebSite.DataModel.ExamineModel;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.Models;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Others;
using CnGalWebSite.DataModel.ViewModel.Search;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CnGalWebSite.APIServer.Application.Helper
{
    public interface IAppHelper
    {

        /// <summary>
        /// 判断该词条部分是否被锁定
        /// </summary>
        /// <param name="entryId">词条Id</param>
        /// <param name="userId">用户Id</param>
        /// <param name="operation">用操作表示部分</param>
        /// <returns>是否被锁定</returns>
        Task<bool> IsEntryLockedAsync(long entryId, string userId, Operation operation);

        /// <summary>
        /// 获取图片路径
        /// </summary>
        /// <param name="image">图片名称</param>
        /// <param name="defaultStr">默认名称</param>
        /// <returns>图片路径</returns>
        string GetImagePath(string image, string defaultStr);

        /// <summary>
        /// 获取字符串的缩略
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="length">缩略长度</param>
        /// <returns>缩略后的字符串 末尾...</returns>
        string GetStringAbbreviation(string str, int length);

        /// <summary>
        /// 判断该 文章 部分是否被锁定
        /// </summary>
        /// <param name="articleId">文章Id</param>
        /// <param name="userId">用户Id</param>
        /// <param name="operation">用操作表示部分</param>
        /// <returns>是否被锁定</returns>
        Task<bool> IsArticleLockedAsync(long articleId, string userId, Operation operation);

        /// <summary>
        /// 获取数字令牌
        /// </summary>
        /// <param name="longToken">字符令牌</param>
        /// <param name="userName">用户名</param>
        /// <returns>数字令牌</returns>
        Task<int> GetShortTokenAsync(string longToken, string userName);
        /// <summary>
        /// 获取字符令牌 失效时间一小时
        /// </summary>
        /// <param name="shortToken">字符令牌</param>
        /// <returns>失效或未找到</returns>
        Task<string> GetLongTokenAsync(string shortToken);
        /// <summary>
        /// 发送验证邮箱 自动转换短令牌
        /// </summary>
        /// <param name="longToken">令牌</param>
        /// <param name="email">邮箱</param>
        /// <param name="userName">用户名</param>
        /// <returns>是否成功发送</returns>
        Task<string> SendVerificationEmailAsync(string longToken, string email, string userName);

        /// <summary>
        /// 在API方法中获取登入用户
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<ApplicationUser> GetAPICurrentUserAsync(HttpContext context);

        /// <summary>
        /// 删除文件 这个文件必须是传入的用户创建的
        /// </summary>
        /// <param name="user">用户</param>
        /// <param name="fileName">文件名</param>
        /// <returns>是否成功</returns>
        Task<bool> DeleteFieAsync(ApplicationUser user, string fileName);

        /// <summary>
        /// 获取在词条编辑页面的简单表示 仅显示层级 例如 游戏->按时长分->短篇
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        Task<string> GetTagListStringAsync(Tag tag);

        /// <summary>
        /// 判断标签是否被锁定
        /// </summary>
        /// <param name="tagId"></param>
        /// <param name="userId"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        Task<bool> IsTagLockedAsync(int tagId, string userId, Operation operation);

        /// <summary>
        /// 增加文章阅读数
        /// </summary>
        /// <param name="articleId">文章</param>
        /// <returns></returns>
        Task ArticleReaderNumUpAsync(long articleId);
        /// <summary>
        /// 增加词条阅读数
        /// </summary>
        /// <param name="entryId">词条</param>
        /// <returns></returns>
        Task EntryReaderNumUpAsync(int entryId);
        /// <summary>
        /// 删除评论 会级联删除所有评论
        /// </summary>
        /// <param name="id">词条Id</param>
        /// <returns></returns>
        Task DeleteComment(long id);
        /// <summary>
        /// 判断用户错误次数是否超过上限
        /// </summary>
        /// <param name="text">唯一标识符</param>
        /// <param name="limit">限制次数</param>
        /// <param name="maxMinutes">限制时间</param>
        /// <returns></returns>
        Task<bool> IsExceedMaxErrorCount(string text, int limit, int maxMinutes);
        /// <summary>
        /// 增加错误次数
        /// </summary>
        /// <param name="text">唯一标识符</param>
        /// <returns></returns>
        Task AddErrorCount(string text);
        /// <summary>
        /// 移除计数器
        /// </summary>
        /// <param name="text">唯一标识符</param>
        /// <returns></returns>
        Task RemoveErrorCount(string text);
        /// <summary>
        /// 判断是否通过人机验证
        /// </summary>
        /// <param name="challenge"></param>
        /// <param name="validate"></param>
        /// <param name="seccode"></param>
        /// <returns></returns>
        bool CheckRecaptcha(string challenge, string validate, string seccode);
        /// <summary>
        /// 获取为卡片展示优化的文章数据模型
        /// </summary>
        /// <param name="item">文章</param>
        /// <returns></returns>
        ArticleInforTipViewModel GetArticleInforTipViewModel(Article item);
        /// <summary>
        /// 获取为卡片展示优化的视频数据模型
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        VideoInforTipViewModel GetVideoInforTipViewModel(Video item);
        /// <summary>
        /// 获取为卡片展示优化的词条数据模型
        /// </summary>
        /// <param name="entry">词条</param>
        /// <returns></returns>
        EntryInforTipViewModel GetEntryInforTipViewModel(Entry entry);
        /// <summary>
        /// 获取为卡片展示优化的标签数据模型
        /// </summary>
        /// <param name="item">标签</param>
        /// <returns></returns>
        TagInforTipViewModel GetTagInforTipViewModel(Tag item);
        /// <summary>
        /// 获取为卡片展示优化的周边数据模型
        /// </summary>
        /// <param name="periphery">周边</param>
        /// <returns></returns>
        PeripheryInforTipViewModel GetPeripheryInforTipViewModel(Periphery periphery);
        /// <summary>
        /// 将json字符串格式化
        /// </summary>
        /// <param name="jsonStr"></param>
        /// <returns></returns>
        string GetJsonStringView(string jsonStr);

        /// <summary>
        /// 将所有没有主图的游戏 设置为steam主图
        /// </summary>
        /// <returns></returns>
        Task GetAllSteamImageToGame();
        /// <summary>
        /// 修改所有图片链接
        /// </summary>
        /// <returns></returns>
        Task ChangeAllImagesPath();
        /// <summary>
        /// 更新收藏夹数目缓存
        /// </summary>
        /// <param name="favoriteFolderIds">收藏夹Id</param>
        /// <returns></returns>
        Task UpdateFavoritesCountAsync(long[] favoriteFolderIds);
        /// <summary>
        /// 发送验证短信
        /// </summary>
        /// <param name="longToken">长令牌</param>
        /// <param name="phone">电话号码</param>
        /// <param name="userName">用户名</param>
        /// <param name="type">验证类型</param>
        /// <returns></returns>
        Task<string> SendVerificationSMSAsync(string longToken, string phone, string userName, SMSType type);
        /// <summary>
        /// 检查发送验证码是否超过上限
        /// </summary>
        /// <param name="mail"></param>
        /// <param name="limit"></param>
        /// <param name="maxMinutes"></param>
        /// <returns></returns>
        Task<bool> IsExceedMaxSendCount(string mail, int limit, int maxMinutes);
        /// <summary>
        /// 增加验证码发送次数
        /// </summary>
        /// <param name="mail"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task AddSendCount(string mail, SendType type);
        /// <summary>
        /// 通过用户Id获取可以标识用户的Key
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="isOneTime">是否一次性有效</param>
        /// <returns></returns>
        Task<string> SetUserLoginKeyAsync(string userId, bool isOneTime = false);
        /// <summary>
        /// 通过key获取用户Id
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<string> GetUserFromLoginKeyAsync(string key);

        /// <summary>
        /// 删除所有互联网档案馆备份记录
        /// </summary>
        /// <returns></returns>
        Task DeleteAllBackupInfor();
        /// <summary>
        /// 检查文本是否合规
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="ip">请求的ip</param>
        /// <returns></returns>
        Task<string> CheckStringCompliance(string text, string ip);
        /// <summary>
        /// 将markdown转化为html
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        string MarkdownToHtml(string str);

        Task<long> GetUserIntegral(string userId, DateTime time);

        bool CheckRecaptcha(HumanMachineVerificationResult model);
    }
}
