using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using Result = CnGalWebSite.DataModel.Model.Result;

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



}
