using System;

namespace CnGalWebSite.HistoryData.Model
{
    internal class DatabaseImportModel
    {
    }
    public partial class Article_Database
    {
        public uint Id { get; set; }
        public sbyte Category { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Content { get; set; }
        public string GameIds { get; set; }
        public string Bgm { get; set; }
        public sbyte State { get; set; }
        public sbyte Published { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public partial class Game
    {
        public uint Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Manufacturer { get; set; }
        public string OfficialSite { get; set; }
        public string SellLink { get; set; }
        public string DownloadLink { get; set; }
        public string ReleaseTime { get; set; }
        public string Content { get; set; }
        public string Tags { get; set; }
        public string Bgm { get; set; }
        public string CoverImg { get; set; }
        public sbyte State { get; set; }
        public sbyte Published { get; set; }
        public sbyte IsTop { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public partial class Guide
    {
        public uint Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Content { get; set; }
        public string GameIds { get; set; }
        public string Bgm { get; set; }
        public sbyte State { get; set; }
        public sbyte Published { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
