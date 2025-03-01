using System;

namespace CnGalWebSite.RobotClientX.Models.Messages
{
    public class GroupMessageRecord
    {
        /// <summary>
        /// 发送者QQ号
        /// </summary>
        public long SenderId { get; set; }

        /// <summary>
        /// 发送者名称
        /// </summary>
        public string SenderName { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime SendTime { get; set; }
    }
}
