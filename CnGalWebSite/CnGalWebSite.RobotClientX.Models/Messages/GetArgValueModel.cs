using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.RobotClientX.Models.Messages
{
    public class GetArgValueModel
    {
        public string Name { get; set; }

        public string Infor { get; set; }

        /// <summary>
        /// 发动方QQ
        /// </summary>
        public long SenderId { get; set; }

        public Dictionary<string, string> AdditionalInformations { get; set; } = new Dictionary<string, string>();

        public string Token { get; set; }
    }
}
