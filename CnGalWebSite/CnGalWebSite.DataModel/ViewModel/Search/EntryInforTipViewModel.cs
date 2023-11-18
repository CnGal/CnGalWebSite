using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CnGalWebSite.DataModel.ViewModel.Search
{
    public class EntryInforTipViewModel
    {
        public int Id { get; set; }

        public EntryType Type { get; set; }

        public string Name { get; set; }

        public string MainImage { get; set; }

        public string BriefIntroduction { get; set; }

        public DateTime LastEditTime { get; set; }
        /// <summary>
        /// 仅游戏词条
        /// </summary>
        public DateTime? PublishTime { get; set; }

        public int ReaderCount { get; set; }

        public int CommentCount { get; set; }

        public List<EntryInforTipAddInforModel> AddInfors { get; set; } = new List<EntryInforTipAddInforModel>() { };

        public List<EditAudioAloneModel> Audio { get; set; } = new List<EditAudioAloneModel>();
    }

    public class EntryInforTipAddInforModel
    {
        public string Modifier { get; set; }

        public List<StaffNameModel> Contents { get; set; } = new List<StaffNameModel>() { };
    }
}
