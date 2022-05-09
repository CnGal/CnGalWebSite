using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CnGalWebSite.PostTools
{
    public class HistoryX
    {
        public readonly List<OutlinkArticleModel> articles = new List<OutlinkArticleModel>();
        public readonly List<MergeEntryModel> mergeEntries = new List<MergeEntryModel>();
        private readonly SettingX setting;
        private readonly HttpClient client;

        public HistoryX(HttpClient _client, SettingX _setting)
        {
            client = _client;
            setting = _setting;
        }

        public void Load()
        {
            try
            {
                var path = Path.Combine(setting.TempPath, "articles.json");

                using (var file = File.OpenText(path))
                {
                    var serializer = new JsonSerializer();
                    articles.AddRange((List<OutlinkArticleModel>)serializer.Deserialize(file, typeof(List<OutlinkArticleModel>)));
                }

                path = Path.Combine(setting.TempPath, "MergeEntries.json");

                using (var file = File.OpenText(path))
                {
                    var serializer = new JsonSerializer();
                    mergeEntries.AddRange((List<MergeEntryModel>)serializer.Deserialize(file, typeof(List<MergeEntryModel>)));
                }

            }
            catch
            {
                Save();
            }

        }
        public void Save()
        {
            var path = Path.Combine(setting.TempPath, "articles.json");

            using (var file = File.CreateText(path))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, articles);
            }

            path = Path.Combine(setting.TempPath, "MergeEntries.json");

            using (var file = File.CreateText(path))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(file, mergeEntries);
            }
        }
    }
}
