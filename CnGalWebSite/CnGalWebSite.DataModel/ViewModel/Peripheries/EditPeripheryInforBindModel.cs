using CnGalWebSite.DataModel.ViewModel.Admin;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel.Peripheries
{
    public class EditPeripheryInforBindModel
    {
        /// <summary>
        /// 审核记录 也是编辑记录
        /// </summary>
        public List<ExaminedNormalListModel> Examines { get; set; } = new List<ExaminedNormalListModel>() { };

        public PeripheryEditState State { get; set; } = new PeripheryEditState();

        public long Id { get; set; }

        public string Name { get; set; }
    }

    public class PeripheryEditState
    {
        public EditState MainState { get; set; }
        public EditState ImagesState { get; set; }
        public EditState RelatedEntriesState { get; set; }
        public EditState RelatedPeripheriesState { get; set; }
    }
}
