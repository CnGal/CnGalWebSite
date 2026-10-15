using System.Text.Json.Serialization;

namespace CnGalWebSite.DataModel.Sorting;

[JsonConverter(typeof(JsonStringEnumConverter<SearchSortField>))]
public enum SearchSortField { Default, Id, LastEditTime, PublishTime, CreateTime, ReaderCount }
