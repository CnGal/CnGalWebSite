using System;

namespace CnGalWebSite.RobotClientX.Models.Messages
{
    public class QQGroupMemberInfo
    {
        /// <summary>
        /// QQ号
        /// </summary>
        public long QQNumber { get; set; }

        /// <summary>
        /// 群昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 所属群号
        /// </summary>
        public long GroupNumber { get; set; }

        public QQGroupMemberInfo()
        {
            NickName = string.Empty;
        }
    }
} 