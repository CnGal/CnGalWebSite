using CnGalWebSite.PublicToolbox.Models;

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
