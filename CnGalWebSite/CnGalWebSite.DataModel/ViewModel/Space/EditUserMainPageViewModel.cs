using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Base;

namespace CnGalWebSite.DataModel.ViewModel.Space
{
    public class EditUserMainPageViewModel:BaseEditModel
    {
        public new string Id { get; set; }

        public string MainPage { get; set; }

        public override Result Validate()
        {
            if (MainPage.Length > 100000)
            {
                return new Result { Successful = false, Error = "文本长度超过上限，强烈建议使用链接显示图片，内嵌图片会导致性能严重下降。请控制文本量，额外的文字可以移动到关联文章中" };
            }
            return base.Validate();
        }
    }
}
