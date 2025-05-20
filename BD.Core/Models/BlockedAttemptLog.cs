namespace BD.Core.Models;

public class BlockedAttemptLog
{
    public string Ip { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsBlocked { get; set; }
    public string UserAgent { get; set; } = string.Empty;
}