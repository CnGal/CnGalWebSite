using Microsoft.Extensions.DependencyInjection;

namespace CnGalWebSite.DrawingBed.Helper.Configuration;

public sealed class ImageApiOptions
{
    public const string SectionName = "ImageApi";
    public string BaseAddress { get; set; } = "https://api.cngal.top/";
}

public static class ImageApiOptionsExtensions
{
    public static IServiceCollection AddImageApiOptions(this IServiceCollection services)
    {
        services.AddOptions<ImageApiOptions>()
            .BindConfiguration(ImageApiOptions.SectionName)
            .Validate(options =>
                Uri.TryCreate(options.BaseAddress, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) &&
                !string.IsNullOrEmpty(uri.Host));
        return services;
    }
}
