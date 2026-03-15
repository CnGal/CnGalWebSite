namespace CnGalWebSite.SDK.MainSite.Models.Files;

/// <summary>
/// 归一化裁剪矩形（0~1 坐标系）
/// </summary>
public sealed record ImageCropRect(double X, double Y, double Width, double Height);
