namespace CnGalWebSite.APIServer.Configuration;

public sealed class DatabaseOptions
{
    public const string SectionName = "ConnectionStrings";
    public string Default { get; set; }
}

public sealed class JwtAuthorityOptions
{
    public const string SectionName = "JwtBearer";
    public string Authority { get; set; }
}

public sealed class InternalApiAccessOptions
{
    public const string SectionName = "InternalApiAccess";
    public string ProjectSiteToken { get; set; }
}

public sealed class ClientIpOptions
{
    public const string SectionName = "ClientIp";
    public string TrustedProxyHosts { get; set; } = "";
}

public sealed class MailOptions
{
    public const string SectionName = "Mail";
    public string Server { get; set; } = "smtp.exmail.qq.com";
    public int Port { get; set; } = 465;
    public string SenderName { get; set; } = "CnGal\u8d44\u6599\u7ad9";
    public string SenderEmail { get; set; } = "kanban@cngal.org";
    public string Account { get; set; } = "kanban@cngal.org";
    public string Password { get; set; }
}

public sealed class MeilisearchOptions
{
    public const string SectionName = "Meilisearch";
    public string BaseAddress { get; set; }
    public string ApiKey { get; set; }
}

public sealed class BackupArchiveOptions
{
    public const string SectionName = "BackupArchive";
    public string BaseAddress { get; set; } = "http://web.archive.org/save/";
}

public sealed class SteamOptions
{
    public const string SectionName = "Steam";
    public string BaseAddress { get; set; } = "https://api.steampowered.com/";
    public string ApiToken { get; set; }
}

public sealed class BilibiliOptions
{
    public const string SectionName = "Bilibili";
    public string Cookie { get; set; }
}

// Separate views of the same section prevent unrelated sources from blocking one another.
public sealed class RssOptions
{
    public const string SectionName = "Rss";
    public string BaseAddress { get; set; } = "https://rss.cngal.top/";
}

public sealed class BilibiliFeedOptions
{
    public const string SectionName = "Rss";
    public string BilibiliFeedAddress { get; set; }
}

public sealed class HeyBoxFeedOptions
{
    public const string SectionName = "Rss";
    public string HeyBoxFeedAddress { get; set; }
}

public sealed class ChatGptOptions
{
    public const string SectionName = "ChatGpt";
    public string BaseAddress { get; set; }
    public string ApiKey { get; set; }
    public int GlobalRequestsPerMinute { get; set; } = 10;
    public string SystemMessageTemplate { get; set; }
    public string UserMessageTemplate { get; set; }
}

public sealed class ChatGptUserLimitsOptions
{
    public const string SectionName = "ChatGpt";
    public int UserRequestsPerMinute { get; set; } = 10;
    public int UserRequestsPerDay { get; set; } = 1000;
    public int MaxMessagesPerConversation { get; set; } = 5;
}

public sealed class IsThereAnyDealOptions
{
    public const string SectionName = "IsThereAnyDeal";
    public string ApiToken { get; set; }
}

public sealed class HeyBoxOptions
{
    public const string SectionName = "HeyBox";
    public string GameDetailBaseAddress { get; set; }
}

public sealed class GamalyticOptions
{
    public const string SectionName = "Gamalytic";
    public string BaseAddress { get; set; }
}

public sealed class VgInsightsOptions
{
    public const string SectionName = "VgInsights";
    public string BaseAddress { get; set; }
}

public sealed class AutomationUsersOptions
{
    public const string SectionName = "AutomationUsers";
    public string NewsAdminId { get; set; } = "30e74a97-12f9-420e-8f90-03aab796fa05";
    public string ExamineAdminId { get; set; } = "a27c27d9-8c42-49f4-a315-05a167e1a6d4";
}

public sealed class GeetestOptions
{
    public const string SectionName = "Geetest";
    public string Id { get; set; }
    public string Key { get; set; }
}
