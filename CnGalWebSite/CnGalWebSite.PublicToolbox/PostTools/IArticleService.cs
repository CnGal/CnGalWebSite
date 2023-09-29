using CnGalWebSite.DataModel.Model;
using CnGalWebSite.PublicToolbox.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.PublicToolbox.PostTools
{
    public interface IArticleService
    {
        Task ProcArticle(RepostArticleModel model, IEnumerable<string> articles, IEnumerable<string> games);

        event Action<KeyValuePair<OutputLevel, string>> ProgressUpdate;
    }
}
