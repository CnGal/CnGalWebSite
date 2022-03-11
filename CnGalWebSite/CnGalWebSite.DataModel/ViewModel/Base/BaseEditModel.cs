using CnGalWebSite.DataModel.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Linq;

namespace CnGalWebSite.DataModel.ViewModel.Base
{
    public class BaseEditModel
    {
        [Display(Name = "唯一名称")]
        public string Name { get; set; }

        public long Id { get; set; }

        [Display(Name = "备注")]
        public string Note { get; set; }

        public virtual Result Validate()
        {
            return new Result
            {
                Successful = true,
            };
        }
    }
}
