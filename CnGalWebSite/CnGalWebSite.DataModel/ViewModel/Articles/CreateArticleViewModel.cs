using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;

namespace CnGalWebSite.DataModel.ViewModel.Articles
{
    public class CreateArticleViewModel : BaseEditModel
    {
        /// <summary>
        /// 主要信息
        /// </summary>
        public EditArticleMainViewModel Main { get; set; } = new EditArticleMainViewModel();
        /// <summary>
        /// 主页
        /// </summary>
        public EditArticleMainPageViewModel MainPage { get; set; } = new EditArticleMainPageViewModel();
        /// <summary>
        /// 关联信息
        /// </summary>
        public EditArticleRelevancesViewModel Relevances { get; set; } = new EditArticleRelevancesViewModel();

        public override Result Validate()
        {
            var result = Main.Validate();
            if (!result.Successful)
            {
                return result;
            }
            result = MainPage.Validate();
            if (!result.Successful)
            {
                return result;
            }
            result = Relevances.Validate();
            if (!result.Successful)
            {
                return result;
            }

            return new Result { Successful = true };
        }
    }
}
