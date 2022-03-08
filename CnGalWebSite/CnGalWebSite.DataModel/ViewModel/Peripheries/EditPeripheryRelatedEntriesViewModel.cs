using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Base;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Peripheries
{
    public class EditPeripheryRelatedEntriesViewModel : BaseEditModel
    {
        public List<RelevancesModel> Roles { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> Staffs { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> Groups { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> Games { get; set; } = new List<RelevancesModel>();

        public override Result Validate()
        {
            if (Staffs.Count == 0 && Games.Count == 0 && Groups.Count == 0 && Roles.Count == 0)
            {
                return new Result { Error = "周边至少需要关联一个词条" };
            }

            return new Result { Successful = true };
        }
    }
}
