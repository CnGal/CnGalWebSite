namespace CnGalWebSite.SDK.MainSite.Abstractions;

public interface IMainSiteAuthRequestService
{
    string NormalizeReturnUrl(string? returnUrl);

    string BuildLoginUrl(string? returnUrl);

    string BuildLogoutUrl(string? returnUrl);
}
