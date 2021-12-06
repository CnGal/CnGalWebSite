using CnGalWebSite.DataModel.Model;

namespace CnGalWebSite.DataModel.ViewModel
{
    public class EditEditionListViewModel
    {
        public EntryType Type { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }

        public List<EditionModel> Editions { get; set; }
    }
}
