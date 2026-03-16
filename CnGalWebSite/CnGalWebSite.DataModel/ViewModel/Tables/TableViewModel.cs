using System;
namespace CnGalWebSite.DataModel.ViewModel.Tables
{
    public class TableViewModel
    {
        public DateTime? LastEditTime { get; set; }

        public int EntriesCount { get; set; }

        public long ArticlesCount { get; set; }

        public int GamesCount { get; set; }

        public int RolesCount { get; set; }

        public int StaffsCount { get; set; }

        public int GroupsCount { get; set; }
    }
}
