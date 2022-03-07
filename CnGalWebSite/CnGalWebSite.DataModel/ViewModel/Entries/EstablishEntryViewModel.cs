using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Entries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel
{
    public class EstablishEntryViewModel: BaseEntryEditModel
    {
        /// <summary>
        /// 主要信息
        /// </summary>
        public EditMainViewModel Main { get; set; } = new EditMainViewModel();
        /// <summary>
        /// 附加信息
        /// </summary>
        public EditAddInforViewModel AddInfor { get; set; } = new EditAddInforViewModel();
        /// <summary>
        /// 主页
        /// </summary>
        public EditMainPageViewModel MainPage { get; set; } = new EditMainPageViewModel();
        /// <summary>
        /// 相册
        /// </summary>
        public EditImagesViewModel Images { get; set; } = new EditImagesViewModel();
        /// <summary>
        /// 关联信息
        /// </summary>
        public EditRelevancesViewModel Relevances { get; set; } = new EditRelevancesViewModel();
        /// <summary>
        /// 标签
        /// </summary>
        public EditEntryTagViewModel Tags { get; set; } = new EditEntryTagViewModel();

    }
}
