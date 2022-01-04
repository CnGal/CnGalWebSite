using CnGalWebSite.DataModel.ViewModel.Search;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Disambig
{
    public class DisambigViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string BriefIntroduction { get; set; }

        public bool IsHidden { get; set; }

        public EditState RelevancesState { get; set; }

        public EditState MainState { get; set; }

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

        public List<DisambigAloneModel> Relecances { get; set; }
    }

    public class DisambigAloneModel
    {
        public EntryInforTipViewModel entry { get; set; }

        public ArticleInforTipViewModel article { get; set; }
    }
}
