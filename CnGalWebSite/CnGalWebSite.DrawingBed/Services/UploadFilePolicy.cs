using CnGalWebSite.DrawingBed.Models.DataModels;
using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace CnGalWebSite.DrawingBed.Services;

public static class UploadFilePolicy
{
    public const string ImageObjectPrefix = "images/upload/validated/";
    public const string AudioObjectPrefix = "audio/upload/";
    public const long MaxImagePixels = 40_000_000;
    public const int MaxImageFrames = 200;
    public const int MaxPngMetadataBytes = 8 * 1024 * 1024;
    private const long MaxDecodedImageBytes = 256 * 1024 * 1024;

    public static void ValidateType(UploadFileType type)
    {
        if (type is not (UploadFileType.Image or UploadFileType.Audio))
            throw new InvalidDataException("不支持的文件类型");
    }

    public static string GetUploadExtension(string fileName, UploadFileType type)
    {
        ValidateType(type);
        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        if (type == UploadFileType.Image)
        {
            GetImageContentType(extension);
        }
        else if (extension is not (".mp3" or ".wav" or ".flac" or ".ogg" or ".opus" or
            ".m4a" or ".aac" or ".wma" or ".aif" or ".aiff"))
        {
            throw new InvalidDataException("不支持的音频格式");
        }
        return extension;
    }

    public static string GetImageContentType(string extension) => extension?.ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        ".bmp" => "image/bmp",
        _ => throw new InvalidDataException("仅支持 PNG、JPEG、GIF、WebP 和 BMP 图片")
    };

    public static async Task<string> NormalizeImageAsync(string source, string expectedExtension = null,
        CancellationToken cancellationToken = default)
    {
        var format = await Image.DetectFormatAsync(source, cancellationToken);
        var extension = format.Name.ToUpperInvariant() switch
        {
            "PNG" => ".png",
            "JPEG" => ".jpg",
            "GIF" => ".gif",
            "WEBP" => ".webp",
            "BMP" => ".bmp",
            _ => throw new InvalidDataException("不支持的图片内容格式")
        };
        if (expectedExtension != null && GetImageContentType(expectedExtension) != GetImageContentType(extension))
            throw new InvalidDataException("图片内容与扩展名不匹配");

        // PNG 文本和颜色配置块会在 Identify 返回前解压，需单独限制元数据大小
        var isAnimated = extension == ".png" && await ValidatePngMetadataAsync(source, cancellationToken);
        var identifyOptions = new DecoderOptions { MaxFrames = extension is ".gif" or ".webp" ? 2u : 1u, SkipMetadata = true };
        var info = await Image.IdentifyAsync(identifyOptions, source, cancellationToken);
        isAnimated |= info.FrameMetadataCollection.Count > 1;
        ValidateDimensions(info.Width, info.Height, 1);
        var pixels = (long)info.Width * info.Height;
        // 索引色图片按至少 RGBA32 估算，16 位 PNG 可能需要 RGBA64
        var frameBytes = pixels * (info.PixelType.BitsPerPixel > 32 ? 8 : 4);
        // ImageSharp 3.x 在第 N 帧前停止 APNG 解码，其他格式会解码第 N 帧
        var extraFrames = isAnimated ? (extension == ".png" ? 2u : 1u) : 0u;
        // 解码前为判断帧数是否超限而额外解码的帧预留内存
        var frameLimit = Math.Min(MaxImageFrames,
            Math.Min(MaxImagePixels / pixels, MaxDecodedImageBytes / frameBytes - extraFrames));
        if (frameLimit < 1)
            throw new InvalidDataException("图片解码内存超出限制");
        var options = new DecoderOptions { MaxFrames = isAnimated ? (uint)frameLimit + extraFrames : 1u };
        using var image = await Image.LoadAsync(options, source, cancellationToken);
        if (image.Frames.Count > frameLimit)
            throw new InvalidDataException("图片尺寸或动画帧数超出限制");
        ValidateDimensions(image.Width, image.Height, image.Frames.Count);
        if (image.Metadata.IccProfile is { } profile && !profile.CheckIsValid())
            image.Metadata.IccProfile = null;
        image.Mutate(context => context.AutoOrient());
        image.Metadata.ExifProfile = null;
        image.Metadata.XmpProfile = null;
        image.Metadata.IptcProfile = null;
        foreach (var frame in image.Frames)
        {
            frame.Metadata.ExifProfile = null;
            frame.Metadata.XmpProfile = null;
            frame.Metadata.IptcProfile = null;
        }
        image.Metadata.GetGifMetadata().Comments.Clear();
        image.Metadata.GetPngMetadata().TextData.Clear();
        var destination = Path.Combine(Path.GetDirectoryName(source), Guid.NewGuid().ToString("N") + extension);
        try
        {
            // 重新编码解码后的像素，避免发布原始文件中附加的可执行内容
            await image.SaveAsync(destination, cancellationToken);
            return destination;
        }
        catch
        {
            File.Delete(destination);
            throw;
        }
    }

    private static async Task<bool> ValidatePngMetadataAsync(string source, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(source);
        stream.Position = 8; // DetectFormat 已校验 PNG 文件签名
        var header = new byte[8];
        var buffer = new byte[8192];
        long metadataBytes = 0;
        var isAnimated = false;
        while (stream.Position < stream.Length)
        {
            await stream.ReadExactlyAsync(header, cancellationToken);
            var length = BinaryPrimitives.ReadUInt32BigEndian(header);
            var type = Encoding.ASCII.GetString(header, 4, 4);
            var next = stream.Position + length + 4L; // 数据块内容及 CRC 校验值
            if (next > stream.Length)
                throw new InvalidDataException("PNG 数据块不完整");
            isAnimated |= type is "acTL" or "fcTL" or "fdAT";
            if (type is "IDAT" or "fdAT")
            {
                stream.Position = next;
                continue;
            }
            metadataBytes += length + 12L;
            if (metadataBytes > MaxPngMetadataBytes)
                throw new InvalidDataException("PNG 元数据超出限制");
            if (type is not ("tEXt" or "zTXt" or "iTXt" or "iCCP"))
            {
                stream.Position = next;
                if (type == "IEND") return isAnimated;
                continue;
            }

            var data = new byte[(int)length];
            await stream.ReadExactlyAsync(data, cancellationToken);
            stream.Position = next;
            var offset = Array.IndexOf(data, (byte)0) + 1;
            // 旧式 EXIF 文本会按声明长度分配内存，实际块大小不能约束该分配
            if (type != "iCCP" && offset > 1 && Encoding.Latin1.GetString(data, 0, offset - 1)
                .Equals("Raw profile type exif", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("不支持 PNG 旧式 EXIF 文本块");
            if (type == "tEXt") continue;
            if (offset <= 1 || offset >= data.Length)
                throw new InvalidDataException("PNG 元数据格式无效");
            if (type == "iTXt")
            {
                if (data[offset] == 0) continue; // 未压缩文本已计入文件内的元数据大小限制
                if (data[offset++] != 1)
                    throw new InvalidDataException("PNG 文本压缩标记无效");
            }
            if (offset >= data.Length || data[offset++] != 0)
                throw new InvalidDataException("PNG 元数据压缩方式无效");
            if (type == "iTXt")
            {
                for (var i = 0; i < 2; i++) // 跳过语言标签和翻译后的关键字
                {
                    var end = Array.IndexOf(data, (byte)0, offset);
                    if (end < 0) throw new InvalidDataException("PNG 文本头不完整");
                    offset = end + 1;
                }
            }
            using var compressed = new MemoryStream(data, offset, data.Length - offset, false);
            using var inflater = new ZLibStream(compressed, CompressionMode.Decompress);
            int read;
            while ((read = await inflater.ReadAsync(buffer, cancellationToken)) > 0)
            {
                metadataBytes += read;
                if (metadataBytes > MaxPngMetadataBytes)
                    throw new InvalidDataException("PNG 元数据超出限制");
            }
        }
        throw new InvalidDataException("PNG 缺少结束数据块");
    }

    private static void ValidateDimensions(int width, int height, int frames)
    {
        if (width <= 0 || height <= 0 || frames > MaxImageFrames ||
            (long)width * height > MaxImagePixels / Math.Max(frames, 1))
            throw new InvalidDataException("图片尺寸或动画帧数超出限制");
    }
}
