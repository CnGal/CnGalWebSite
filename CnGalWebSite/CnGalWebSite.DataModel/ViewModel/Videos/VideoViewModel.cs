using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.DataModel.ViewModel.Space;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Videos
{
    public class VideoViewModel
    {
        public long Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// 简介
        /// </summary>
        public string BriefIntroduction { get; set; }
        /// <summary>
        /// 主图
        /// </summary>
        public string MainPicture { get; set; }
        /// <summary>
        /// 背景图
        /// </summary>
        public string BackgroundPicture { get; set; }
        /// <summary>
        /// 小背景图
        /// </summary>
        public string SmallBackgroundPicture { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 版权
        /// </summary>
        public CopyrightType Copyright { get; set; }
        /// <summary>
        /// 时长
        /// </summary>
        public TimeSpan Duration { get; set; }
        /// <summary>
        /// 是否为互动视频
        /// </summary>
        public bool IsInteractive { get; set; }
        /// <summary>
        /// 是否为用户本人创作
        /// </summary>
        public bool IsCreatedByCurrentUser { get; set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 最后编辑时间
        /// </summary>
        public DateTime LastEditTime { get; set; }
        /// <summary>
        /// 发布时间 指转载前发布时间
        /// </summary>
        public DateTime PubishTime { get; set; }
        /// <summary>
        /// 创建视频的用户
        /// </summary>
        public ApplicationUser CreateUser { get; set; }
        public string CreateUserId { get; set; }

        /// <summary>
        /// 阅读数
        /// </summary>
        public int ReaderCount { get; set; }
        /// <summary>
        /// 点赞数
        /// </summary>
        public int ThumbsUpCount { get; set; }
        /// <summary>
        /// 评论数
        /// </summary>
        public int CommentCount { get; set; }


        /// <summary>
        /// 是否隐藏
        /// </summary>
        public bool IsHidden { get; set; } = false;

        /// <summary>
        /// 是否可以评论
        /// </summary>
        public bool? CanComment { get; set; } = true;

        /// <summary>
        /// 原作者名称
        /// </summary>
        public string OriginalAuthor { get; set; }

        /// <summary>
        /// 是否有权限编辑
        /// </summary>
        public bool Authority { get; set; }
        /// <summary>
        /// 是否处于编辑状态
        /// </summary>
        public bool IsEdit { get; set; }
        /// <summary>
        /// 是否已经点赞
        /// </summary>
        public bool IsThumbsUp { get; set; }

        /// <summary>
        /// 主页
        /// </summary>
        public string MainPage { get; set; }


        public EditState MainState { get; set; }
        public EditState MainPageState { get; set; }
        public EditState RelevancesState { get; set; }
        public EditState ImagesState { get; set; }

        /// <summary>
        /// 相关性列表
        /// </summary>
        public List<EntryInforTipViewModel> RelatedEntries { get; set; } = new List<EntryInforTipViewModel>();

        public List<ArticleInforTipViewModel> RelatedArticles { get; set; } = new List<ArticleInforTipViewModel>();

        public List<VideoInforTipViewModel> RelatedVideos { get; set; } = new List<VideoInforTipViewModel>();


        /// <summary>
        /// 外部链接
        /// </summary>
        public List<RelevancesKeyValueModel> RelatedOutlinks { get; set; } = new List<RelevancesKeyValueModel> { };

        /// <summary>
        /// 用户信息
        /// </summary>
        public UserInforViewModel UserInfor { get; set; } = new UserInforViewModel();


        /// <summary>
        /// 图片列表
        /// </summary>
        public List<PicturesViewModel> Pictures { get; set; } = new List<PicturesViewModel>();
    }
}
