using CnGalWebSite.DataModel.ViewModel.Base;
using System.Collections.Generic;

namespace CnGalWebSite.DataModel.ViewModel.Articles
{
    public class EditArticleRelevancesViewModel : BaseEditModel
    {

        public List<RelevancesModel> Roles { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> Staffs { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> Groups { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> Games { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> Articles { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> Videos { get; set; } = new List<RelevancesModel>();
        public List<RelevancesModel> Others { get; set; } = new List<RelevancesModel>();
    }
}
