using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Tags;
using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Articles
{
    public class EditArticleInforBindModel
    {
        /// <summary>
        /// 审核记录 也是编辑记录
        /// </summary>
        public List<ExaminedNormalListModel> Examines { get; set; } = new List<ExaminedNormalListModel>() { };

        public ArticleEditState State { get; set; } = new ArticleEditState();

        public long Id { get; set; }

        public string Name { get; set; }
    }

    public class ArticleEditState
    {
        public EditState MainState { get; set; }
        public EditState MainPageState { get; set; }
        public EditState RelevancesState { get; set; }
    }
}
