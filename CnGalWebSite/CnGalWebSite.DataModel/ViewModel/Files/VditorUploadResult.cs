using System;
using System.Collections.Generic;
using System.Text;

namespace CnGalWebSite.DataModel.ViewModel.Files
{
    public class VditorUploadResult
    {
        public string msg { get; set; }

        public int code { get; set; } = 200;

        public VditorUploadResultData date { get; set; } = new VditorUploadResultData();
    }

    public class VditorUploadResultData
    {
        public List<string> errFiles { get; set; } = new List<string>();

        public List<KeyValuePair<string, string>> succMap { get; set; } = new List<KeyValuePair<string, string>>();
    }

    public enum VditorUploadResultCode
    {
        Fail,
        Success,
    }
}
