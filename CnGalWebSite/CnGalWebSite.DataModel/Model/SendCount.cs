using System.ComponentModel.DataAnnotations;

using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.Model
{
    public class SendCount
    {
        public long Id { get; set; }

        public string Mail { get; set; }

        public string IP { get; set; }

        public SendType Type { get; set; }

        public DateTime SendTime { get; set; }
    }

    public enum SendType
    {
        [Display(Name = "电子邮件")]
        Email,
        [Display(Name = "短信")]
        Phone
    }
}
