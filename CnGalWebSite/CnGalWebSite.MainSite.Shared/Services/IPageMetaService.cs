namespace CnGalWebSite.MainSite.Shared.Services;

/// <summary>
/// 页面级 SEO meta 状态传递服务（Scoped）。
/// CgPageMeta 组件在 OnParametersSet 中将值写入此服务。
/// App.razor 渲染 &lt;head&gt; 时直接读取并渲染 &lt;meta&gt; 标签。
/// SetMeta 仅在值发生变更时自动通知订阅者重新渲染。
/// </summary>
public interface IPageMetaService
{
    string Title { get; }
    string Description { get; }
    string Image { get; }
    event Action? OnChanged;

    /// <summary>设置页面 meta 信息，值变更时自动触发 OnChanged。</summary>
    void SetMeta(string? title, string? description, string? image);
}

public sealed class PageMetaService : IPageMetaService
{
    private const string DefaultTitle = "CnGal 中文GalGame资料站";
    private const string DefaultDescription = "CnGal资料站的建站目的是收集，索引国产gal及中文化galgame资料、文章、攻略，为galgame同好们提供方便。";
    private const string DefaultImage = "https://app.cngal.org/_content/CnGalWebSite.Shared/images/logo.png";

    private string _title = DefaultTitle;
    private string _description = DefaultDescription;
    private string _image = DefaultImage;

    public string Title => _title;
    public string Description => _description;
    public string Image => _image;

    public event Action? OnChanged;

    public void SetMeta(string? title, string? description, string? image)
    {
        var newTitle = string.IsNullOrWhiteSpace(title) ? DefaultTitle : $"{title} - {DefaultTitle}";
        var newDescription = string.IsNullOrWhiteSpace(description) ? DefaultDescription : description;
        var newImage = string.IsNullOrWhiteSpace(image) ? DefaultImage : image;

        if (_title != newTitle || _description != newDescription || _image != newImage)
        {
            _title = newTitle;
            _description = newDescription;
            _image = newImage;
            OnChanged?.Invoke();
        }
    }
}
