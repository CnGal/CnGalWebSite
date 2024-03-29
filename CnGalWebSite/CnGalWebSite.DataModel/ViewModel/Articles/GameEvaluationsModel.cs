﻿using CnGalWebSite.DataModel.ViewModel.Search;
using System.Collections.Generic;

namespace CnGalWebSite.DataModel.ViewModel.Articles
{
    public class GameEvaluationsModel
    {
        public string Name { get; set; }

        public int Id { get; set; }

        public List<ArticleInforTipViewModel> Evaluations { get; set; } = new List<ArticleInforTipViewModel>();
    }
}
