using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.PostTools;
using CnGalWebSite.Helper.Helper;
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
