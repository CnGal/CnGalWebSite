using CnGalWebSite.RobotClientX.Models.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Timer = System.Timers.Timer;

namespace CnGalWebSite.RobotClientX.Services.Messages
{
    public class GroupMessageCacheService : IGroupMessageCacheService, IDisposable
    {
        private readonly ConcurrentDictionary<long, List<GroupMessageRecord>> _messageCache;
        private readonly ILogger<GroupMessageCacheService> _logger;
        private readonly Timer _cleanupTimer;
        private int _expirationMinutes;

        public GroupMessageCacheService(ILogger<GroupMessageCacheService> logger)
        {
            _messageCache = new ConcurrentDictionary<long, List<GroupMessageRecord>>();
            _logger = logger;
            _expirationMinutes = 30; // 默认30分钟过期

            // 设置定时清理任务，每分钟执行一次
            _cleanupTimer = new Timer(60 * 1000); // 60秒
            _cleanupTimer.Elapsed += (s, e) => CleanExpiredMessages();
            _cleanupTimer.Start();
        }

        public void AddMessage(long groupId, GroupMessageRecord message)
        {
            _messageCache.AddOrUpdate(
                groupId,
                new List<GroupMessageRecord> { message },
                (key, existingList) =>
                {
                    existingList.Add(message);
                    return existingList;
                });
        }

        public List<GroupMessageRecord> GetGroupMessages(long groupId)
        {
            return _messageCache.TryGetValue(groupId, out var messages)
                ? messages.ToList()
                : new List<GroupMessageRecord>();
        }

        public void CleanExpiredMessages()
        {
            try
            {
                var now = DateTime.Now;
                foreach (var groupId in _messageCache.Keys)
                {
                    if (_messageCache.TryGetValue(groupId, out var messages))
                    {
                        var validMessages = messages
                            .Where(m => (now - m.SendTime).TotalMinutes <= _expirationMinutes)
                            .ToList();

                        _messageCache.TryUpdate(groupId, validMessages, messages);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理过期消息时发生错误");
            }
        }

        public void SetExpirationTime(int minutes)
        {
            if (minutes <= 0)
            {
                throw new ArgumentException("过期时间必须大于0分钟");
            }
            _expirationMinutes = minutes;
        }

        public void ClearGroupMessages(long groupId)
        {
            _messageCache.TryRemove(groupId, out _);
        }

        public void KeepLatestMessages(long groupId, int count)
        {
            if (count <= 0)
            {
                throw new ArgumentException("保留的消息数量必须大于0");
            }

            if (_messageCache.TryGetValue(groupId, out var messages))
            {
                var latestMessages = messages
                    .OrderByDescending(m => m.SendTime)
                    .Take(count)
                    .OrderBy(m => m.SendTime)
                    .ToList();

                _messageCache.TryUpdate(groupId, latestMessages, messages);
            }
        }

        public void Dispose()
        {
            _cleanupTimer?.Dispose();
        }
    }
}
