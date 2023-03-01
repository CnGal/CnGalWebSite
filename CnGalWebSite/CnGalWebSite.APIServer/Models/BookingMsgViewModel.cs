namespace CnGalWebSite.APIServer.Models
{
    public class BookingMsgViewModel
    {
        public string DisplayName { get; set; }

        public string Link { get; set; }

        public string MainPicture { get; set; }

        public string BriefIntroduction { get; set; }

        public List<string> Pictures { get; set; } = new List<string>();



    }
}
