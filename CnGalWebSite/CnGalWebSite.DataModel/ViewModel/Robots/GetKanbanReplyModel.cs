using CnGalWebSite.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Robots
{
    public class GetKanbanReplyModel
    {
        public string Message { get; set; }

        public bool IsFirst { get; set; }

        public DeviceIdentificationModel Identification { get; set; } = new DeviceIdentificationModel();

    }
}
