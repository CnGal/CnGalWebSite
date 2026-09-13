using CnGalWebSite.DrawingBed.Helper.Configuration;
using Microsoft.Extensions.Options;
using MySqlConnector;
using System.Net;

namespace CnGalWebSite.APIServer.Configuration;

public static class ApiOptionsRegistration
{
    public static IServiceCollection AddApiConfiguration(this IServiceCollection services)
    {
        services.AddOptions<DatabaseOptions>()
            .BindConfiguration(DatabaseOptions.SectionName)
            .Validate(IsValidDatabase, "ConnectionStrings is missing or invalid.")
            .ValidateOnStart();

        services.AddOptions<JwtAuthorityOptions>()
            .BindConfiguration(JwtAuthorityOptions.SectionName)
            .Validate(options => IsHttpAddress(options.Authority),
                "JwtBearer:Authority is missing or invalid.")
            .ValidateOnStart();

        services.AddOptions<InternalApiAccessOptions>()
            .BindConfiguration(InternalApiAccessOptions.SectionName)
            .Validate(options => IsRequired(options.ProjectSiteToken),
                "InternalApiAccess:ProjectSiteToken is missing.");

        services.AddOptions<ClientIpOptions>()
            .BindConfiguration(ClientIpOptions.SectionName);

        services.AddOptions<MailOptions>()
            .BindConfiguration(MailOptions.SectionName)
            .PostConfigure<IConfiguration>((options, configuration) =>
            {
                // An explicitly blank account opts into anonymous SMTP instead of restoring the default.
                var account = configuration["Mail:Account"];
                if (account is not null && string.IsNullOrWhiteSpace(account))
                    options.Account = "";
            })
            .Validate(IsValidMail, "Mail configuration is missing or invalid.");

        services.AddOptions<MeilisearchOptions>()
            .BindConfiguration(MeilisearchOptions.SectionName)
            .PostConfigure(options =>
            {
                if (string.IsNullOrWhiteSpace(options.ApiKey))
                    options.ApiKey = null;
            })
            .Validate(options => IsHttpAddress(options.BaseAddress) &&
                (string.IsNullOrEmpty(options.ApiKey) || IsHttpHeaderValue(options.ApiKey)));

        services.AddImageApiOptions();

        services.AddOptions<BackupArchiveOptions>()
            .BindConfiguration(BackupArchiveOptions.SectionName)
            .Validate(options => IsHttpAddress(options.BaseAddress));

        services.AddOptions<SteamOptions>()
            .BindConfiguration(SteamOptions.SectionName)
            .Validate(options => IsHttpAddress(options.BaseAddress) && IsRequired(options.ApiToken));

        services.AddOptions<BilibiliOptions>()
            .BindConfiguration(BilibiliOptions.SectionName)
            .Validate(options => IsHttpHeaderValue(options.Cookie));

        services.AddOptions<RssOptions>()
            .BindConfiguration(RssOptions.SectionName)
            .Validate(options => IsHttpAddress(options.BaseAddress));

        services.AddOptions<BilibiliFeedOptions>()
            .BindConfiguration(BilibiliFeedOptions.SectionName)
            .Validate(options => IsHttpAddress(options.BilibiliFeedAddress));

        services.AddOptions<HeyBoxFeedOptions>()
            .BindConfiguration(HeyBoxFeedOptions.SectionName)
            .Validate(options => IsHttpAddress(options.HeyBoxFeedAddress));

        services.AddOptions<ChatGptOptions>()
            .BindConfiguration(ChatGptOptions.SectionName)
            .Validate(IsValidChatGpt);

        services.AddOptions<ChatGptUserLimitsOptions>()
            .BindConfiguration(ChatGptUserLimitsOptions.SectionName)
            .Validate(IsValidChatGptLimits);

        services.AddOptions<IsThereAnyDealOptions>()
            .BindConfiguration(IsThereAnyDealOptions.SectionName)
            .Validate(options => IsRequired(options.ApiToken));

        services.AddOptions<HeyBoxOptions>()
            .BindConfiguration(HeyBoxOptions.SectionName)
            .Validate(options => IsHttpAddress(options.GameDetailBaseAddress));

        services.AddOptions<GamalyticOptions>()
            .BindConfiguration(GamalyticOptions.SectionName)
            .Validate(options => IsHttpAddress(options.BaseAddress));

        services.AddOptions<VgInsightsOptions>()
            .BindConfiguration(VgInsightsOptions.SectionName)
            .Validate(options => IsHttpAddress(options.BaseAddress));

        services.AddOptions<AutomationUsersOptions>()
            .BindConfiguration(AutomationUsersOptions.SectionName)
            .Validate(options => IsRequired(options.NewsAdminId) &&
                IsRequired(options.ExamineAdminId));

        services.AddOptions<GeetestOptions>()
            .BindConfiguration(GeetestOptions.SectionName)
            .Validate(options => IsRequired(options.Id) && IsRequired(options.Key));

        return services;
    }

    private static bool IsValidDatabase(DatabaseOptions options)
    {
        if (!IsRequired(options.Default))
            return false;

        try
        {
            var builder = new MySqlConnectionStringBuilder(options.Default);
            return IsRequired(builder.Server) &&
                IsRequired(builder.Database) &&
                (builder.ConnectionProtocol != MySqlConnectionProtocol.Sockets ||
                 builder.Port is >= 1 and <= 65535);
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or
            OverflowException or InvalidCastException)
        {
            return false;
        }
    }

    private static bool IsValidMail(MailOptions options)
    {
        if (!IsRequired(options.Server) || options.Port is < 1 or > 65535 ||
            !IsRequired(options.SenderName))
            return false;

        try
        {
            _ = new MimeKit.MailboxAddress(options.SenderName, options.SenderEmail);
        }
        catch (Exception ex) when (ex is MimeKit.ParseException or ArgumentException)
        {
            return false;
        }

        return (!string.IsNullOrWhiteSpace(options.Account) &&
                !string.IsNullOrWhiteSpace(options.Password)) ||
            (string.IsNullOrWhiteSpace(options.Account) &&
             string.IsNullOrWhiteSpace(options.Password));
    }

    private static bool IsValidChatGpt(ChatGptOptions options) =>
        IsHttpAddress(options.BaseAddress) &&
        IsHttpHeaderValue(options.ApiKey) &&
        IsRequired(options.SystemMessageTemplate) &&
        IsRequired(options.UserMessageTemplate) &&
        options.GlobalRequestsPerMinute > 0;

    private static bool IsValidChatGptLimits(ChatGptUserLimitsOptions options) =>
        options.UserRequestsPerMinute > 0 &&
        options.UserRequestsPerDay > 0 &&
        options.MaxMessagesPerConversation > 0;

    private static bool IsRequired(string value) =>
        !string.IsNullOrWhiteSpace(value);

    private static bool IsHttpAddress(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) &&
        !string.IsNullOrEmpty(uri.Host);

    private static bool IsHttpHeaderValue(string value) =>
        IsRequired(value) && !value.Contains('\r') &&
        !value.Contains('\n') && !value.Contains('\0');
}
