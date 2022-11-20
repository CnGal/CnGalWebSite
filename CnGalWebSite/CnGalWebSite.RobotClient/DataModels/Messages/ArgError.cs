using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.RobotClient.DataModels.Messages
{
    public class ArgError : Exception
    {
        public string Error { get; set; }

        public ArgError(string error)
        {
            Error = error;
        }
    }
}
