using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.ViewModel.Home;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.DataModel.ViewModel.Tags;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.ThematicPages
{
    public class CVThematicPageViewModel
    {
        public List<TagTreeModel> Tags { get; set; } = new List<TagTreeModel>();

        public List<CVInforViewModel> CVInfors { get; set; } = new List<CVInforViewModel>();

        public List<HomeNewsAloneViewModel> News { get; set; } = new List<HomeNewsAloneViewModel>();

        public List<CarouselViewModel> Carousels { get; set; } = new List<CarouselViewModel>();

        #region  数据缓存

        public List<string> SelectedTags { get; set; }= new List<string>();

        public List<string>  UnlabeledTags { get; set; } = new List<string>();

        public int CurrentPage { get; set; } = 1;

        public CVThematicPageSortType SortType { get; set; }

        public string SearchString { get; set; }

        #endregion
    }

    public class CVInforViewModel
    {
        public EntryInforTipViewModel Infor { get; set; } = new EntryInforTipViewModel();
        /// <summary>
        /// 标签列表
        /// </summary>
        public List<TagsViewModel> Tags { get; set; } = new List<TagsViewModel>();

        public List<EditAudioAloneModel> Audio { get; set; } = new List<EditAudioAloneModel>();

        public DateTime? LastUploadAudioTime { get; set; }

        public int WorkCount { get; set; }

        public bool IsCertificated { get; set; }

        /// <summary>
        /// 最近发布作品时间
        /// </summary>
        public DateTime? LastPublishTime { get; set; }
    }

    public enum CVThematicPageSortType
    {
        [Display(Name = "默认")]
        None,
        [Display(Name = "随机")]
        Random,
        [Display(Name = "热度")]
        ReadCount,
        [Display(Name = "参与作品数")]
        WorkCount,
        [Display(Name = "最近发布")]
        RecentlyPublish,
    }

}
