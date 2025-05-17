
using CnGalWebSite.RobotClientX.DataModels;
using CnGalWebSite.RobotClientX.Models.Messages;
using CnGalWebSite.RobotClientX.Models.Robots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.RobotClientX.Services.QQClients
{
    public interface IQQClientService
    {
        Task SendMessage(SendMessageModel model);

        Task SendMessage(RobotReplyRange range, long id, string text);

        Task Init();

        string GetMiraiSession();
    }
}
