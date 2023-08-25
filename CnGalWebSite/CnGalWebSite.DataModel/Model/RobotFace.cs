using System;

namespace CnGalWebSite.DataModel.Model
{
    [Obsolete("已从主站拆分")]
    public class RobotFace
    {
        public long Id { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        public string Note { get; set; }

        public bool IsHidden { get; set; }
    }
}
