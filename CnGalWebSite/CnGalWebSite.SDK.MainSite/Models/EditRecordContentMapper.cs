using CnGalWebSite.DataModel.ViewModel;
using CnGalWebSite.DataModel.ViewModel.Admin;
using CnGalWebSite.DataModel.ViewModel.Articles;
using CnGalWebSite.DataModel.ViewModel.Peripheries;
using CnGalWebSite.DataModel.ViewModel.Tags;
using CnGalWebSite.DataModel.ViewModel.Videos;
using CnGalWebSite.Extensions;

namespace CnGalWebSite.SDK.MainSite.Models;

/// <summary>
/// 历史快照到审核预览模型的投影。实现已上移到 <see cref="RecordContentMapper"/>，
/// 与 APIServer 的历史回放共用同一份映射；此处保留公开入口以兼容既有 SDK 调用方。
/// </summary>
public static class EditRecordContentMapper
{
    public static ExaminePreDataModel Map(EntryIndexViewModel value) => RecordContentMapper.Map(value);
    public static ExaminePreDataModel Map(ArticleViewModel value) => RecordContentMapper.Map(value);
    public static ExaminePreDataModel Map(VideoViewModel value) => RecordContentMapper.Map(value);
    public static ExaminePreDataModel Map(TagIndexViewModel value) => RecordContentMapper.Map(value);
    public static ExaminePreDataModel Map(PeripheryViewModel value) => RecordContentMapper.Map(value);
}
