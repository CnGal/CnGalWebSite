using System.Globalization;
using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.Peripheries;
using CnGalWebSite.DataModel.ViewModel.Search;
using CnGalWebSite.DataModel.ViewModel.Tags;
using CnGalWebSite.DataModel.ViewModel.Videos;
using CnGalWebSite.Extensions;

namespace CnGalWebSite.SDK.MainSite.Models;

/// <summary>Explicit projections keep historical content separate from live counters and permissions.</summary>
public static class EditRecordContentMapper
{
    public static ExaminePreDataModel Map(EntryIndexViewModel value) => RecordContentMapper.Map(value);
    public static ExaminePreDataModel Map(ArticleViewModel value) => RecordContentMapper.Map(value);
    public static ExaminePreDataModel Map(VideoViewModel value) => RecordContentMapper.Map(value);
    public static ExaminePreDataModel Map(TagIndexViewModel value) => RecordContentMapper.Map(value);
    public static ExaminePreDataModel Map(PeripheryViewModel value) => RecordContentMapper.Map(value);
}
