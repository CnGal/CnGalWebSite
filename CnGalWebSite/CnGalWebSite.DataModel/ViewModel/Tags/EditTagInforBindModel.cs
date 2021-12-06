using CnGalWebSite.DataModel.ViewModel.Admin;

namespace CnGalWebSite.DataModel.ViewModel.Tags
{
    public class EditTagInforBindModel
    {
        /// <summary>
        /// 审核记录 也是编辑记录
        /// </summary>
        public List<ExaminedNormalListModel> Examines { get; set; } = new List<ExaminedNormalListModel>() { };

        public TagEditState State { get; set; } = new TagEditState();

        public long Id { get; set; }

        public string Name { get; set; }
    }

    public class TagEditState
    {
        public EditState MainState { get; set; }
        public EditState ChildTagsState { get; set; }
        public EditState ChildEntriesState { get; set; }
    }
}
