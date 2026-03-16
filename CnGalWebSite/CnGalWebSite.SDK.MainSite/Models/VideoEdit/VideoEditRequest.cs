using CnGalWebSite.DataModel.ViewModel.Videos;

namespace CnGalWebSite.SDK.MainSite.Models.VideoEdit;

public sealed class VideoEditRequest
{
    public bool IsCreate { get; set; }

    public CreateVideoViewModel Data { get; set; } = new();
}
