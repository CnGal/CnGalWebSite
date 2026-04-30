using CnGalWebSite.MainSite.Shared.Models.Toolbox;
using CnGalWebSite.SDK.MainSite.Abstractions;

namespace CnGalWebSite.MainSite.Shared.Services.Toolbox;

public sealed class ToolboxImageService(
    IFileCommandService fileCommandService,
    IToolboxLocalRepository<OriginalImageMapModel> imageRepository)
{
    public async Task<string> GetImageAsync(string url, double x = 0, double y = 0, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return string.Empty;
        }

        if (!url.Contains("http", StringComparison.OrdinalIgnoreCase))
        {
            url = "https:" + url;
        }

        var cached = (await imageRepository.GetAllAsync(cancellationToken))
            .FirstOrDefault(s => s.OldUrl == url && s.X == x && s.Y == y);
        if (cached is not null && !string.IsNullOrWhiteSpace(cached.NewUrl) && cached.NewUrl != url)
        {
            return cached.NewUrl;
        }

        var result = await fileCommandService.TransformImageUrlAsync(url, x, y, cancellationToken);
        if (!result.Success || string.IsNullOrWhiteSpace(result.Data))
        {
            return url;
        }

        if (result.Data != url)
        {
            await imageRepository.InsertAsync(new OriginalImageMapModel
            {
                OldUrl = url,
                NewUrl = result.Data,
                X = x,
                Y = y
            }, cancellationToken);
        }

        return result.Data;
    }
}
