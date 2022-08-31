using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Others;
using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Space
{
    public class EditUserCertificationModel
    {
        public bool IsPending { get; set; }

        public string EntryName { get; set; }

        public EntryType Type { get; set; }

        public string Note { get; set; } = "可以上传并填写一些有助于我们确认身份的内容哦~";

        public HumanMachineVerificationResult Verification { get; set; }
    }
}
