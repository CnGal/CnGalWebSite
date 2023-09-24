﻿using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Ranks;
using CnGalWebSite.DataModel.ViewModel.Space;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Admin
{
    public class ExaminedNormalListModel
    {
        public long Id { get; set; }

        public ExaminedNormalListModelType Type { get; set; }

        public DateTime ApplyTime { get; set; }

        public DateTime? PassedTime { get; set; }

        public string RelatedId { get; set; }

        public string RelatedName { get; set; }

        public Operation Operation { get; set; }

        public bool? IsPassed { get; set; }

        public UserInforViewModel UserInfor { get; set; } = new UserInforViewModel();
    }

    public enum ExaminedNormalListModelType
    {
        [Display(Name = "词条")]
        Entry,
        [Display(Name = "文章")]
        Article,
        [Display(Name = "用户")]
        User,
        [Display(Name = "标签")]
        Tag,
        [Display(Name = "评论")]
        Comment,
        [Display(Name = "消歧义页")]
        Disambig,
        [Display(Name = "周边")]
        Periphery,
        [Display(Name = "游玩记录")]
        PlayedGame,
        [Display(Name = "用户认证")]
        UserCertification,
        [Display(Name = "视频")]
        Video,
        [Display(Name = "收藏夹")]
        FavoriteFolder
    }

    public enum ExaminedNormalListPassType
    {
        [Display(Name = "全部")]
        All,
        [Display(Name = "已通过")]
        Passed,
        [Display(Name = "待审核")]
        Passing,
        [Display(Name = "未通过")]
        UnPassed
    }

    public enum ExaminedNormalListSortType
    {
        [Display(Name = "编辑时间")]
        EditTime,
        [Display(Name = "审核时间")]
        ExamineTime,
    }
}
