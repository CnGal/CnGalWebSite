namespace CnGalWebSite.HistoryData.Model
{
    public class NameIdPairModel
    {
        public long Id { get; set; }

        public string Name { get; set; }
    }

    public class OldAndNewUrlModel
    {
        public string OldUrl { get; set; }

        public string NewUrl { get; set; }

        public string Title { get; set; }
    }
}
