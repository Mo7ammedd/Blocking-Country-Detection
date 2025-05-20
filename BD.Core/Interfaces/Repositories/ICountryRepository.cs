namespace BD.Core.Interfaces.Repositories;

using BD.Core.Models;

public interface ICountryRepository
{
    bool AddBlockedCountry(string countryCode, string countryName);
    bool RemoveBlockedCountry(string countryCode);
    List<Country> GetBlockedCountries(int page = 1, int pageSize = 10, string? searchTerm = null);
    bool IsCountryBlocked(string countryCode);
    
    bool AddTemporalBlock(string countryCode, string countryName, int durationMinutes);
    bool RemoveExpiredTemporalBlocks();
    
    bool CountryExists(string countryCode);
}