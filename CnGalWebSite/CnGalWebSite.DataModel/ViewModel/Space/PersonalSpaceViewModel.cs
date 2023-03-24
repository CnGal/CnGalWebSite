﻿using CnGalWebSite.DataModel.ViewModel.Search;
using System;
using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Space
{
    public class PersonalSpaceViewModel
    {
        public string Id { get; set; }

        public string Email { get; set; }

        public string MainPageContext { get; set; }

        public DateTime? Birthday { get; set; }

        public string Role { get; set; }

        public bool IsCurrentUser { get; set; }

        public int ContributionValue { get; set; }

        public int EditEntryNum { get; set; }

        public int CreateArticleNum { get; set; }

        public DateTime? LastEditTime { get; set; }

        public DateTime RegisteTime { get; set; }

        public long TotalExamine { get; set; }
        public DateTime LastOnlineTime { get; set; }

        /// <summary>
        /// 在线时间 单位 小时
        /// </summary>
        public double OnlineTime { get; set; }

        public List<KeyValuePair<DateTime, int>> EditCountList { get; set; } = new List<KeyValuePair<DateTime, int>>();
        public List<KeyValuePair<DateTime, int>> SignInDaysList { get; set; } = new List<KeyValuePair<DateTime, int>>();

        public long TotalFilesSpace { get; set; }
        public long UsedFilesSpace { get; set; }

        public int PassedExamineCount { get; set; }
        public int UnpassedExamineCount { get; set; }
        public int PassingExamineCount { get; set; }

        public bool CanComment { get; set; }

        public bool IsShowFavorites { get; set; }

        public bool IsShowGameRecord { get; set; }

        public string SteamId { get; set; }

        /// <summary>
        /// 用户基础信息
        /// </summary>
        public UserInforViewModel BasicInfor { get; set; } = new UserInforViewModel();
        /// <summary>
        /// 用户认证词条
        /// </summary>
        public EntryInforTipViewModel UserCertification { get; set; } = new EntryInforTipViewModel();

    }
}
