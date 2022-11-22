using CnGalWebSite.DataModel.ViewModel.Admin;
using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Videos
{
    public class EditVideoInforBindModel
    {
        /// <summary>
        /// 审核记录 也是编辑记录
        /// </summary>
        public List<ExaminedNormalListModel> Examines { get; set; } = new List<ExaminedNormalListModel>() { };

        public VideoEditState State { get; set; } = new VideoEditState();

        public long Id { get; set; }

        public string Name { get; set; }
    }

    public class VideoEditState
    {
        public EditState MainState { get; set; }
        public EditState MainPageState { get; set; }
        public EditState RelevancesState { get; set; }
        public EditState ImagesState { get; set; }
    }
}
