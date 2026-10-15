using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CnGalWebSite.Core.Models;

namespace CnGalWebSite.DataModel.ViewModel.Dtos;

public class PagedAndFilterInput
{
    [Range(0, 10000)]
    public int MaxResultCount { get; set; } = 10;
    public int CurrentPage { get; set; } = 1;
    public string FilterText { get; set; }
    public string ScreeningConditions { get; set; }
}

public class PagedSortedAndFilterInput<TSort> : PagedAndFilterInput, IValidatableObject where TSort : struct, Enum
{
    public IReadOnlyList<SortOption<TSort>> Sorts { get; set; } = Array.Empty<SortOption<TSort>>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Sorts == null || Sorts.Any(sort => sort == null || !Enum.IsDefined(sort.Field) || !Enum.IsDefined(sort.Direction)))
            yield return new ValidationResult("排序字段或方向无效", new[] { nameof(Sorts) });
    }
}
