using CnGalWebSite.PublicToolbox.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.PublicToolbox.PostTools
{
    public interface IVideoService
    {
        Task ProcVideo(RepostVideoModel model, IEnumerable<string> videos, IEnumerable<string> games);

        event Action<KeyValuePair<OutputLevel, string>> ProgressUpdate;
    }
}
