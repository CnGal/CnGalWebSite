using CnGalWebSite.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Models.ViewModels.Stalls
{
    public class ApplyStallModel
    {
        public long StallId { get; set; }

        public bool Apply { get; set; }


        /// <summary>
        /// 身份识别数据
        /// </summary>
        public DeviceIdentificationModel Identification { get; set; } = new DeviceIdentificationModel();

    }
}
