using CnGalWebSite.DataModel.ViewModel.Admin;
using System.ComponentModel.DataAnnotations;

namespace CnGalWebSite.DataModel.ViewModel.Space
{
    public class UserPendingDataModel
    {
        public string Id { get; set; }

        /// <summary>
        /// 目标链接 仅限评论
        /// </summary>
        public string Link { get; set; }

        public string Name { get; set; }

        public string BriefIntroduction { get; set; }

        public string MainPicture { get; set; }

        public string Thumbnail { get; set; }

        public ExaminedNormalListModelType Type { get; set; }

        public UserPendingDataDisplayMode DisplayMode { get; set; }
    }

    public enum UserPendingDataDisplayMode
    {
        [Display(Name = "主图模式")]
        Main,
        [Display(Name = "缩略图模式")]
        Thum,
        [Display(Name = "文本模式")]
        Text
    }

}
