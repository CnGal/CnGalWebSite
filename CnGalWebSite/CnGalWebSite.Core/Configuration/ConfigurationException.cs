namespace CnGalWebSite.Core.Configuration;

/// <summary>A deployment error containing configuration paths, never configuration values.</summary>
public sealed class ConfigurationException(string section)
    : Exception($"Missing or invalid configuration in section '{section}'.")
{
    public string Section { get; } = section;
}
