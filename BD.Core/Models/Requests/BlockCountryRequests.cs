namespace BD.Core.Models.Requests;

public class BlockCountryRequest
{
    public string CountryCode { get; set; } = string.Empty;
}

public class TemporalBlockCountryRequest
{
    public string CountryCode { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
}