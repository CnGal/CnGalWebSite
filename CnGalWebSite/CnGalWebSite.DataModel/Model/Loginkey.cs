using System;
namespace CnGalWebSite.DataModel.Model
{
    public class Loginkey
    {
        public long Id { get; set; }

        public string UserId { get; set; }

        public string Key { get; set; }

        public bool IsOneTime { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
