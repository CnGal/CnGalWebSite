namespace CnGalWebSite.DataModel.ViewModel.PlayedGames
{
    public class HiddenGameRecordModel
    {
        public long[] PlayedGameIds { get; set; } = new long[0];

        public int?[] GameIds { get; set; } = new int?[0];

        public bool IsHidden { get; set; }
    }
}
