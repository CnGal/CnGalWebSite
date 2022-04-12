using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.RobotClient
{
    public class ClientX
    {
        private readonly Setting _setting;
        public MeowMiraiLib.Client MiraiClient { get; set; }

        public ClientX(Setting setting)
        {
            _setting = setting;
        }

        public void Init()
        {
            MiraiClient = new($"ws://{_setting.MiraiUrl}/all?verifyKey={_setting.VerifyKey}&qq={_setting.QQ}", true, true, -1);
        }
    }
}
