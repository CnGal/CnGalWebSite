using CnGalWebSite.RobotClientX.Models.Messages;
using System.Collections.Concurrent;

namespace CnGalWebSite.RobotClientX.Services
{
    public interface IQQGroupMemberCacheService
    {
        /// <summary>
        /// 更新群成员列表
        /// </summary>
        void UpdateGroupMembers(long groupNumber, List<QQGroupMemberInfo> members);

        /// <summary>
        /// 获取群成员昵称
        /// </summary>
        string GetMemberNickName(long groupNumber, long qqNumber);

        /// <summary>
        /// 检查群成员列表是否需要更新
        /// </summary>
        bool NeedUpdate(long groupNumber);
    }

    public class QQGroupMemberCacheService : IQQGroupMemberCacheService
    {
        private readonly ConcurrentDictionary<long, Dictionary<long, string>> _groupMembersCache = new();
        private readonly ConcurrentDictionary<long, DateTime> _lastUpdateTime = new();
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1);

        public void UpdateGroupMembers(long groupNumber, List<QQGroupMemberInfo> members)
        {
            var memberDict = members.ToDictionary(m => m.QQNumber, m => m.NickName);
            _groupMembersCache.AddOrUpdate(groupNumber, memberDict, (_, _) => memberDict);
            _lastUpdateTime.AddOrUpdate(groupNumber, DateTime.Now, (_, _) => DateTime.Now);
        }

        public string GetMemberNickName(long groupNumber, long qqNumber)
        {
            if (_groupMembersCache.TryGetValue(groupNumber, out var groupMembers))
            {
                if (groupMembers.TryGetValue(qqNumber, out var nickName))
                {
                    return nickName;
                }
            }
            return string.Empty;
        }

        public bool NeedUpdate(long groupNumber)
        {
            if (!_lastUpdateTime.TryGetValue(groupNumber, out var lastUpdate))
            {
                return true;
            }

            return DateTime.Now - lastUpdate > _cacheExpiration;
        }
    }
} 