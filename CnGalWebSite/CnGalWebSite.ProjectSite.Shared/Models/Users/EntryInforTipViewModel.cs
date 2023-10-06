using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Shared.Models.Users
{
    public class EntryInforTipViewModel
    {
        public int Id { get; set; }
        public EntryType Type { get; set; }
        public string Name { get; set; }
        public string MainImage { get; set; }
        public string BriefIntroduction { get; set; }
    }

    public enum EntryType
    {
        Game,
        Role,
        ProductionGroup,
        Staff
    }

}
