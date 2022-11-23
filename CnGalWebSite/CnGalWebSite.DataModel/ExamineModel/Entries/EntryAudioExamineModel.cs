using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Linq;

namespace CnGalWebSite.DataModel.ExamineModel.Entries
{
    public class EntryAudioExamineModel
    {
        public List<EntryAudioExamineAloneModel> Audio { get; set; } = new List<EntryAudioExamineAloneModel>();
    }

    public class EntryAudioExamineAloneModel
    {
        public bool IsDelete { get; set; }
        [Display(Name = "名称")]
        public string Name { get; set; }
        [Display(Name = "简介")]
        public string BriefIntroduction { get; set; }
        [Display(Name = "链接")]
        public string Url { get; set; }
        [Display(Name = "优先级")]
        public int Priority { get; set; }
        [Display(Name = "时长")]
        public TimeSpan Duration { get; set; }
        [Display(Name = "缩略图")]
        public string Thumbnail { get; set; }
    }
}
