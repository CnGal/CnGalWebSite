namespace CnGalWebSite.DataModel.ViewModel.Tags
{
    public class TagContrastEditRecordViewModel
    {
        public long ContrastId { get; set; }
        public long CurrentId { get; set; }

        public TagIndexViewModel ContrastModel { get; set; }
        public TagIndexViewModel CurrentModel { get; set; }
    }
}
