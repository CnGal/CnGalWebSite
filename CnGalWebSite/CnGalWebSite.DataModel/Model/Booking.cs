using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.Model
{
    public class Booking
    {
        public long Id { get; set; }

        /// <summary>
        /// 是否需要通知
        /// </summary>
        public bool IsNeedNotification { get; set; }

        /// <summary>
        /// 是否开启预约
        /// </summary>
        public bool Open { get; set; }

        /// <summary>
        /// 关联抽奖 用户预约后自动参加
        /// </summary>
        public int LotteryId { get; set; }

        public int BookingCount { get; set; }

        public int? EntryId { get; set; }
        public Entry Entry { get; set; }

        public virtual ICollection<BookingUser> Users { get; set; } = new List<BookingUser>();

        public virtual ICollection<BookingGoal> Goals { get; set; } = new List<BookingGoal>();
    }

    public class BookingGoal
    {
        public long Id { get; set; }

        /// <summary>
        /// 唯一名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 当前目标达成的最低人数
        /// </summary>
        public int Target { get; set; }
    }

    public class BookingUser
    {
        public long Id { get; set; }

        /// <summary>
        /// 是否已通知
        /// </summary>
        public bool IsNotified { get; set; }

        public DateTime BookingTime { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
        public string ApplicationUserId { get; set; }

        public Booking Booking { get; set; }
        public long? BookingId { get; set; }
    }
}
