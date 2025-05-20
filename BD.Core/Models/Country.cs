namespace BD.Core.Models;

public class Country
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime? BlockedUntil { get; set; }
    public bool IsPermanentlyBlocked { get; set; }
    
    public bool IsBlocked => IsPermanentlyBlocked || (BlockedUntil.HasValue && BlockedUntil > DateTime.UtcNow);
}