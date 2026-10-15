using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CnGalWebSite.Core.Models;

[JsonConverter(typeof(JsonStringEnumConverter<SortDirection>))]
public enum SortDirection
{
    Ascending,
    Descending
}

public sealed record SortOption<TSort>(TSort Field, SortDirection Direction = SortDirection.Descending)
    where TSort : struct, Enum;

public class QueryParameterModel<TSort> : IValidatableObject where TSort : struct, Enum
{
    public string SearchText { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int ItemsPerPage { get; set; } = 10;
    public IReadOnlyList<SortOption<TSort>> Sorts { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Sorts == null || Sorts.Any(sort => sort == null || !Enum.IsDefined(sort.Field) || !Enum.IsDefined(sort.Direction)))
            yield return new ValidationResult("排序字段或方向无效", [nameof(Sorts)]);
    }
}

public class QueryResultModel<TModel, TSort> where TModel : class where TSort : struct, Enum
{
    public IEnumerable<TModel> Items { get; set; } = [];
    public QueryParameterModel<TSort> Parameter { get; set; } = new();
    public int Total { get; set; }
}
