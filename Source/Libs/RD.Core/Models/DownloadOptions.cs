namespace RD.Core.Models;

public class DownloadOptions
{
    public string Url { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int MaxSegments { get; set; } = 8;
    public int BufferSize { get; set; } = 8192;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
    public int RetryAttempts { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(2);
    public bool OverwriteExisting { get; set; } = false;
    public bool EnableResume { get; set; } = true;
    public ProxyConfiguration? Proxy { get; set; }
    public AuthenticationConfiguration? Authentication { get; set; }
}

public class ProxyConfiguration
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool UseDefaultCredentials { get; set; } = false;
}

public class AuthenticationConfiguration
{
    public AuthenticationType Type { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Token { get; set; }
    public string? ApiKey { get; set; }
    public string? HeaderName { get; set; } = "Authorization";
}

public enum AuthenticationType
{
    None,
    Basic,
    Bearer,
    ApiKey,
    Custom
}