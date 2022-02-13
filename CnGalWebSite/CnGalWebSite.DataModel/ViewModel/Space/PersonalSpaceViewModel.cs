using System;
using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Space
{
    public class PersonalSpaceViewModel
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public string MainPageContext { get; set; }

        public string PersonalSignature { get; set; }

        public string Id { get; set; }

        public string PhotoPath { get; set; }

        public DateTime? Birthday { get; set; }

        public string Role { get; set; }

        public bool IsCurrentUser { get; set; }

        public int SignInDays { get; set; }

        public bool IsSignIn { get; set; }

        public int Integral { get; set; }

        public int Level { get; set; }

        public int ContributionValue { get; set; }

        public bool IsExamineList { get; set; }

        public int EditEntryNum { get; set; }

        public int CreateArticleNum { get; set; }

        public DateTime? LastEditTime { get; set; }

        public DateTime RegisteTime { get; set; }

        public string BackgroundImagePath { get; set; }
        public long TotalExamine { get; set; }
        public DateTime LastOnlineTime { get; set; }
        /// <summary>
        /// 用户背景图 大屏幕
        /// </summary>
        public string MBgImage { get; set; }

        /// <summary>
        /// 用户背景图 小屏幕
        /// </summary>
        public string SBgImage { get; set; }

        /// <summary>
        /// 在线时间 单位 小时
        /// </summary>
        public double OnlineTime { get; set; }

        public List<KeyValuePair<DateTime, int>> EditCountList { get; set; }
        public List<KeyValuePair<DateTime, int>> SignInDaysList { get; set; }

        public long TotalFilesSpace { get; set; }
        public long UsedFilesSpace { get; set; }

        public int PassedExamineCount { get; set; }
        public int UnpassedExamineCount { get; set; }
        public int PassingExamineCount { get; set; }

        public bool CanComment { get; set; }

        public bool IsShowFavorites { get; set; }

        public string SteamId { get; set; }

    }
}
