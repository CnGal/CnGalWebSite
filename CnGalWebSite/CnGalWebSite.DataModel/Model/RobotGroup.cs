using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.Model
{
    public class RobotGroup
    {
        public long Id { get; set; }

        public long GroupId { get; set; }

        public bool IsHidden { get; set; }

        public string Note { get; set; }

    }
}
