namespace CnGalWebSite.DataModel.ExamineModel
{
    public class EntryTagsModel_1_0
    {
        public List<TagModel> Tags = new();
    }

    public class TagModel
    {
        public string Name { get; set; }

        public bool IsDelete { get; set; }
    }

}
