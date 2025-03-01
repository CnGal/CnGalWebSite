using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CnGalWebSite.Kanban.ChatGPT.Models.Functions
{
    public class SearchModel
    {
        [JsonPropertyName("total_count")]
        public int TotalCount { get; set; }

        [JsonPropertyName("current_page")]
        public int CurrentPage { get; set; }

        [JsonPropertyName("total_page")]
        public int TotalPage { get; set; }

        [JsonPropertyName("items")]
        public List<SearchItemModel> Items { get; set; } = [];
    }

    public class SearchItemModel
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("main_type")]
        public string? MainType { get; set; }

        [JsonPropertyName("sub_type")]
        public string? SubType { get; set; }

        [JsonPropertyName("brief")]
        public string? BriefIntroduction { get; set; }

        public long Id { get; set; }
    }
}
