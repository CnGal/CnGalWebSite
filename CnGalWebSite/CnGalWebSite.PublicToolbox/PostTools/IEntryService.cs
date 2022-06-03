using CnGalWebSite.DataModel.ViewModel.PostTools;
using CnGalWebSite.Helper.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.PublicToolbox.PostTools
{
    public interface IEntryService
    {
        event Action<KeyValuePair<OutputLevel, string>> ProgressUpdate;

        Task ProcMergeEntry(MergeEntryModel model);
    }
}
