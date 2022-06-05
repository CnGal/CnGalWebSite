using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Search;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Anniversaries
{
    public class JudgableGameViewModel
    {
        public int Id { get; set; }
       
        public EntryType Type { get; set; }
       
        public string Name { get; set; }
        
        public string DisplayName { get; set; }
        
        public string MainImage { get; set; }
       
        public string BriefIntroduction { get; set; }

        public DateTime PublishTime { get; set; }

        public DateTime LastEditTime { get; set; }
        /// <summary>
        /// 最后评分时间
        /// </summary>
        public DateTime LastScoreTime { get; set; }
        /// <summary>
        /// 评价数
        /// </summary>
        public int ScoreCount { get; set; }
        
        public int ReaderCount { get; set; }
        
        public int CommentCount { get; set; }
    }
}
