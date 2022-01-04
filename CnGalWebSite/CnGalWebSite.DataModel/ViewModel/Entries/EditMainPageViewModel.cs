using CnGalWebSite.DataModel.Model;
using System.ComponentModel.DataAnnotations;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.ViewModel
{
    public class EditMainPageViewModel
    {
        public string Context { get; set; }

        public string Name { get; set; }

        public int Id { get; set; }

        public EntryType Type { get; set; }

        [Display(Name = "备注")]
        public string Note { get; set; }

    }
}
