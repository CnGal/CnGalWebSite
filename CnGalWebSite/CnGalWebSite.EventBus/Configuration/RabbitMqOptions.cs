namespace CnGalWebSite.EventBus.Configuration;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";
    public string HostName { get; set; }
    public int Port { get; set; } = 5672;
    public string UserName { get; set; }
    public string Password { get; set; }

    public static bool IsValid(RabbitMqOptions options)
    {
        return !string.IsNullOrWhiteSpace(options.HostName) &&
            options.Port is >= 1 and <= 65535 &&
            !string.IsNullOrWhiteSpace(options.UserName) &&
            !string.IsNullOrWhiteSpace(options.Password);
    }
}
