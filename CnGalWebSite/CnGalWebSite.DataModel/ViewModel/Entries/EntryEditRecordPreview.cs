using CnGalWebSite.DataModel.Model;
using CnGalWebSite.DataModel.ViewModel.Ranks;
using System;
using System.Collections.Generic;

namespace CnGalWebSite.DataModel.ViewModel.Entries;

public sealed record EntryEditRecordPreview(int TotalCount, IReadOnlyList<EntryEditRecordItem> Items)
{
    public const int PageSize = 8;
}

public sealed record EntryEditRecordItem(
    long Id,
    DateTime ApplyTime,
    Operation Operation,
    string AuthorName,
    string AuthorPhoto,
    IReadOnlyList<RankViewModel> AuthorRanks);
