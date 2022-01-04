using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.Model
{
    public class Rank
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Text { get; set; }

        public string CSS { get; set; }

        public string Styles { get; set; }

        public bool IsHidden { get; set; } = false;

        public int Priority { get; set; } = 0;

        public ICollection<RankUser> RankUsers { get; set; }


        public DateTime CreateTime { get; set; }
        public DateTime LastEditTime { get; set; }
    }

    public class RankUser
    {
        public long Id { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public long RankId { get; set; }
        public Rank Rank { get; set; }

        public DateTime Time { get; set; }

        public bool IsHidden { get; set; } = false;

    }
}
