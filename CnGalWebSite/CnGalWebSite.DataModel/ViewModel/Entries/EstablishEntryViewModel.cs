using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Entries;
namespace CnGalWebSite.DataModel.ViewModel
{
    public class EstablishEntryViewModel : BaseEntryEditModel
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
        /// <summary>
        /// 音频
        /// </summary>
        public EditAudioViewModel Audio { get; set; } = new EditAudioViewModel();

        public override Result Validate()
        {
            var result = Main.Validate();
            if (!result.Successful)
            {
                return result;
            }
            result = AddInfor.Validate();
            if (!result.Successful)
            {
                return result;
            }
            result = MainPage.Validate();
            if (!result.Successful)
            {
                return result;
            }
            result = Images.Validate();
            if (!result.Successful)
            {
                return result;
            }
            result = Relevances.Validate();
            if (!result.Successful)
            {
                return result;
            }
            result = Tags.Validate();
            if (!result.Successful)
            {
                return result;
            }
            result = Audio.Validate();
            if (!result.Successful)
            {
                return result;
            }

            return new Result { Successful = true };
        }

    }
}
