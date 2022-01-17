using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Perfections;
using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Entries
{
    public class EditEntryInforBindModel
    {
        public PerfectionInforTipViewModel PerfectionInfor { get; set; } = new PerfectionInforTipViewModel();
        /// <summary>
        /// 审核记录 也是编辑记录
        /// </summary>
        public List<ExaminedNormalListModel> Examines { get; set; } = new List<ExaminedNormalListModel>() { };

        public List<PerfectionCheckViewModel> PerfectionChecks { get; set; } = new List<PerfectionCheckViewModel>();

        public EntryEditState State { get; set; } = new EntryEditState();

        public int Id { get; set; }
    }

    public class EntryEditState
    {
        public EditState MainState { get; set; }
        public EditState MainPageState { get; set; }
        public EditState ImagesState { get; set; }
        public EditState RelevancesState { get; set; }
        public EditState InforState { get; set; }
        public EditState TagState { get; set; }
    }
}
