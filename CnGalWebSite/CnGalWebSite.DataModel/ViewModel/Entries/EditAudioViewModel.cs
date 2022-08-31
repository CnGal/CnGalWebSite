using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace CnGalWebSite.DataModel.ViewModel.Entries
{
    public class EditAudioViewModel : BaseEntryEditModel
    {
        public List<EditAudioAloneModel> Audio { get; set; } = new List<EditAudioAloneModel>();

        public override Result Validate()
        {

            foreach (var item in Audio)
            {
                if (Audio.Count(s => s.Url == item.Url) > 1)
                {
                    return new Result { Error = $"{item.Name} 与其他音频重复了，链接：{item.Url}", Successful = false };

                }
            }

            return base.Validate();
        }
    }

    public class EditAudioAloneModel
    {
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
