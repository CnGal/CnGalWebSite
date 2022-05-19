using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Entries;
namespace CnGalWebSite.DataModel.ViewModel
{
    public class EditMainPageViewModel : BaseEntryEditModel
    {
        public string Context { get; set; }

        public override Result Validate()
        {
            //检查文本长度
            if (Context != null && Context.Length > 100000)
            {
                return new Result { Error = "文本长度超过上限，强烈建议使用链接显示图片，内嵌图片会导致性能严重下降。请控制文本量，额外的文字可以移动到关联文章中" };
            }
            return new Result { Successful = true };
        }

    }
}
