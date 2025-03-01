using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.RobotClientX.Services.GPT
{
    public interface IChatGPTService
    {
        Task<string> GetReply(long sendTo);
    }
}
