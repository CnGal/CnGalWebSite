using CnGalWebSite.Core.Models;
using CnGalWebSite.DataModel.ViewModel.Others;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.DataModel.ViewModel.Commodities
{
    public class RedeemedCommodityCodeModel
    {
        public string Code { get; set; }

        public HumanMachineVerificationResult Verification { get; set; }

        public DeviceIdentificationModel Identification { get; set; } = new DeviceIdentificationModel();
    }
}
