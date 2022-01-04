using System;
 using System.Collections.Generic;
namespace CnGalWebSite.DataModel.Model
{
    public class ThirdPartyLoginInfor
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string UniqueId { get; set; }

        public ThirdPartyLoginType Type { get; set; }

        public string ApplicationUserId { get; set; }

        public ApplicationUser ApplicationUser { get; set; }
    }

    public enum ThirdPartyLoginType
    {
        Microsoft,
        GitHub,
        Gitee,
        QQ
    }
}
