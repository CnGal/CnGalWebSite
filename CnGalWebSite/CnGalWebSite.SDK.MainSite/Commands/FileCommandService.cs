using System.Net.Http.Json;
using CnGalWebSite.SDK.MainSite.Abstractions;
using CnGalWebSite.SDK.MainSite.Models;
using CnGalWebSite.SDK.MainSite.Models.Files;
using Microsoft.Extensions.Logging;

namespace CnGalWebSite.SDK.MainSite.Commands;

public sealed class FileCommandService(
    HttpClient httpClient,
    ILogger<FileCommandService> logger) : IFileCommandService
{
    public async Task<SdkResult<string>> UploadImageAsync(
        Stream fileStream,
        string fileName,
        ImageAspectType aspectType = ImageAspectType.None,
        bool gallery = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var streamContent = new StreamContent(fileStream);
            using var form = new MultipartFormDataContent();
            form.Add(streamContent, "\"files\"", fileName);

            var (x, y) = GetAspectRatio(aspectType);
            var requestUri = $"api/files/Upload?x={x}&y={y}&type=0&gallery={gallery.ToString().ToLowerInvariant()}";
            var response = await httpClient.PostAsync(requestUri, form, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("图片上传失败。Status={StatusCode}; Path={Path}; BaseAddress={BaseAddress}",
                    (int)response.StatusCode, requestUri, httpClient.BaseAddress);
                return SdkResult<string>.Fail("FILE_UPLOAD_HTTP_ERROR", $"上传失败（HTTP {(int)response.StatusCode}）");
            }

            var results = await response.Content.ReadFromJsonAsync<List<UploadResultDto>>(cancellationToken: cancellationToken);
            var first = results?.FirstOrDefault();
            if (first is null)
            {
                return SdkResult<string>.Fail("FILE_UPLOAD_EMPTY_RESPONSE", "上传失败，服务未返回结果");
            }

            if (!first.Uploaded || string.IsNullOrWhiteSpace(first.Url))
            {
                return SdkResult<string>.Fail("FILE_UPLOAD_FAILED", first.Error ?? "上传失败");
            }

            return SdkResult<string>.Ok(first.Url);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "图片上传异常。FileName={FileName}; BaseAddress={BaseAddress}", fileName, httpClient.BaseAddress);
            return SdkResult<string>.Fail("FILE_UPLOAD_EXCEPTION", "上传图片时发生异常");
        }
    }

    private static (double x, double y) GetAspectRatio(ImageAspectType type)
    {
        return type switch
        {
            ImageAspectType._1_1 => (1, 1),
            ImageAspectType._16_9 => (460, 215),
            ImageAspectType._9_16 => (9, 16),
            ImageAspectType._4_1A2 => (4, 1.2),
            _ => (0, 0)
        };
    }

    public async Task<SdkResult<AudioUploadResult>> UploadAudioAsync(
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var streamContent = new StreamContent(fileStream);
            using var form = new MultipartFormDataContent();
            form.Add(streamContent, "\"files\"", fileName);

            // type=1 corresponds to UploadFileType.Audio
            var requestUri = "api/files/Upload?type=1";
            var response = await httpClient.PostAsync(requestUri, form, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("音频上传失败。Status={StatusCode}; Path={Path}; BaseAddress={BaseAddress}",
                    (int)response.StatusCode, requestUri, httpClient.BaseAddress);
                return SdkResult<AudioUploadResult>.Fail("AUDIO_UPLOAD_HTTP_ERROR", $"上传失败（HTTP {(int)response.StatusCode}）");
            }

            var results = await response.Content.ReadFromJsonAsync<List<AudioUploadResponseDto>>(cancellationToken: cancellationToken);
            var first = results?.FirstOrDefault();
            if (first is null)
            {
                return SdkResult<AudioUploadResult>.Fail("AUDIO_UPLOAD_EMPTY_RESPONSE", "上传失败，服务未返回结果");
            }

            if (!first.Uploaded || string.IsNullOrWhiteSpace(first.Url))
            {
                return SdkResult<AudioUploadResult>.Fail("AUDIO_UPLOAD_FAILED", first.Error ?? "上传失败");
            }

            return SdkResult<AudioUploadResult>.Ok(new AudioUploadResult
            {
                Url = first.Url,
                FileName = first.FileName,
                Duration = first.Duration ?? TimeSpan.Zero
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "音频上传异常。FileName={FileName}; BaseAddress={BaseAddress}", fileName, httpClient.BaseAddress);
            return SdkResult<AudioUploadResult>.Fail("AUDIO_UPLOAD_EXCEPTION", "上传音频时发生异常");
        }
    }

    private sealed class UploadResultDto
    {
        public bool Uploaded { get; set; }
        public string? Url { get; set; }
        public string? Error { get; set; }
    }

    private sealed class AudioUploadResponseDto
    {
        public bool Uploaded { get; set; }
        public string? Url { get; set; }
        public string? FileName { get; set; }
        public TimeSpan? Duration { get; set; }
        public string? Error { get; set; }
    }
}
