using CnGalWebSite.DataModel.ViewModel.Videos;

namespace CnGalWebSite.SDK.MainSite.Models.VideoEdit;

public sealed class VideoEditViewModel
{
    public bool IsCreate { get; set; }

    public CreateVideoViewModel Data { get; set; } = new();

    public VideoEditMetaOptions MetaOptions { get; set; } = new();
}
