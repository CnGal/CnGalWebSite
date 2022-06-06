using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CnGalWebSite.DataModel.Model
{
    public class OperationRecord
    {
        public long Id { get; set; }

        public OperationRecordType Type { get; set; }

        public string ObjectId { get; set; }

        public string Ip { get; set; }

        public string Cookie { get; set; }
    }

    public enum OperationRecordType
    {
        [Display(Name ="投票")]
        Vote,
        [Display(Name = "抽奖")]
        Lottery,
        [Display(Name = "评分")]
        Score
    }
}
