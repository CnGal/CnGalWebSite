using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Videos
{
    public class CreateVideoViewModel : BaseEditModel
    {
        /// <summary>
        /// 主要信息
        /// </summary>
        public EditVideoMainViewModel Main { get; set; } = new EditVideoMainViewModel();
        /// <summary>
        /// 主页
        /// </summary>
        public EditVideoMainPageViewModel MainPage { get; set; } = new EditVideoMainPageViewModel();
        /// <summary>
        /// 关联信息
        /// </summary>
        public EditVideoRelevancesViewModel Relevances { get; set; } = new EditVideoRelevancesViewModel();
        /// <summary>
        /// 相册
        /// </summary>
        public EditVideoImagesViewModel Images { get; set; } = new EditVideoImagesViewModel();

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
            result = Images.Validate();
            if (!result.Successful)
            {
                return result;
            }

            return new Result { Successful = true };
        }
    }
}
