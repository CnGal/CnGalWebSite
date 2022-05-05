using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Entries;
using System.Collections.Generic;
using System.Linq;

namespace CnGalWebSite.DataModel.ViewModel
{
    public class EditEntryTagViewModel : BaseEntryEditModel
    {
        public List<RelevancesModel> Tags { get; set; } = new List<RelevancesModel>();

        public override Result Validate()
        {
            if (Tags.Any(s => s.DisplayName == "游戏" || s.DisplayName == "角色" || s.DisplayName == "STAFF" || s.DisplayName == "制作组"))
            {
                return new Result { Error = "不能添加顶级标签，请查看《词条编辑规范》" };
            }
            return new Result { Successful = true };
        }
    }
}
