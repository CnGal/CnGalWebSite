using CnGalWebSite.RobotClientX.Models.Messages;
using System;
using System.Collections.Generic;

namespace CnGalWebSite.RobotClientX.Services.Messages
{
    public interface IGroupMessageCacheService
    {
        /// <summary>
        /// 添加群消息记录
        /// </summary>
        void AddMessage(long groupId, GroupMessageRecord message);

        /// <summary>
        /// 获取指定群的所有消息记录
        /// </summary>
        List<GroupMessageRecord> GetGroupMessages(long groupId);

        /// <summary>
        /// 清理过期的消息记录
        /// </summary>
        void CleanExpiredMessages();

        /// <summary>
        /// 设置消息过期时间（分钟）
        /// </summary>
        void SetExpirationTime(int minutes);

        /// <summary>
        /// 清空指定群的所有消息记录
        /// </summary>
        void ClearGroupMessages(long groupId);

        /// <summary>
        /// 只保留指定群的最新n条消息记录
        /// </summary>
        /// <param name="groupId">群号</param>
        /// <param name="count">要保留的消息数量</param>
        void KeepLatestMessages(long groupId, int count);
    }
}
