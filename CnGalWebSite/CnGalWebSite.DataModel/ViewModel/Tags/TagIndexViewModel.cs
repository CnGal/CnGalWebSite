using CnGalWebSite.DataModel.ViewModel.Search;

namespace CnGalWebSite.DataModel.ViewModel.Tags
{
    public class TagIndexViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<KeyValuePair<string, int>> Taglevels { get; set; } = new List<KeyValuePair<string, int>>();
        /// <summary>
        /// 父标签
        /// </summary>
        public TagInforTipViewModel ParentTag { get; set; } = new TagInforTipViewModel();

        public string BriefIntroduction { get; set; }

        public bool IsHidden { get; set; }

        public bool IsEdit { get; set; }

        public EditState MainState { get; set; }

        public EditState ChildEntriesState { get; set; }

        public EditState ChildTagsState { get; set; }

        /// <summary>
        /// 阅读数
        /// </summary>
        public int ReaderCount { get; set; }

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
        /// 缩略图
        /// </summary>
        public string Thumbnail { get; set; }

        /// <summary>
        /// 关联词条
        /// </summary>
        public List<EntryInforTipViewModel> ChildrenEntries { get; set; } = new List<EntryInforTipViewModel>() { };

        /// <summary>
        /// 子标签
        /// </summary>
        public List<TagInforTipViewModel> ChildrenTags { get; set; } = new List<TagInforTipViewModel>();
    }
}
