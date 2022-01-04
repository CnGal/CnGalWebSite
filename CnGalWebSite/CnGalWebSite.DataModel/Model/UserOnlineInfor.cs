using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.Model
{
    public class UserOnlineInfor
    {
        public long Id { get; set; }

        public DateTime Date { get; set; }

        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

    }
}
