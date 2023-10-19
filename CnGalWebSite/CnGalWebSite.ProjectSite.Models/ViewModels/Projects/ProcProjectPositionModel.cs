using CnGalWebSite.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.ProjectSite.Models.ViewModels.Projects
{
    public class ProcProjectPositionModel
    {
        public long UserId { get; set; }

        public bool Passed { get; set; }

        /// <summary>
        /// 身份识别数据
        /// </summary>
        public DeviceIdentificationModel Identification { get; set; } = new DeviceIdentificationModel();

    }
}
