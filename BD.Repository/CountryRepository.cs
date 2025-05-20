namespace BD.Repository;

using System.Collections.Concurrent;
using BD.Core.Interfaces.Repositories;
using BD.Core.Models;

public class CountryRepository : ICountryRepository
{
    private readonly ConcurrentDictionary<string, Country> _blockedCountries = new();

    public bool AddBlockedCountry(string countryCode, string countryName)
    {
        var country = new Country
        {
            Code = countryCode,
            Name = countryName,
            IsPermanentlyBlocked = true
        };

        return _blockedCountries.TryAdd(countryCode, country);
    }

    public bool RemoveBlockedCountry(string countryCode)
    {
        return _blockedCountries.TryRemove(countryCode, out _);
    }

    public List<Country> GetBlockedCountries(int page = 1, int pageSize = 10, string? searchTerm = null)
    {
        var query = _blockedCountries.Values.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c => 
                c.Code.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || 
                c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        return query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public bool IsCountryBlocked(string countryCode)
    {
        if (_blockedCountries.TryGetValue(countryCode, out var country))
        {
            return country.IsBlocked;
        }
        return false;
    }

    public bool AddTemporalBlock(string countryCode, string countryName, int durationMinutes)
    {
        var expirationTime = DateTime.UtcNow.AddMinutes(durationMinutes);

        if (_blockedCountries.TryGetValue(countryCode, out var existingCountry))
        {
            if (existingCountry.IsPermanentlyBlocked)
            {
                return false;
            }

            if (existingCountry.BlockedUntil.HasValue && existingCountry.BlockedUntil.Value >= expirationTime)
            {
                return false;
            }

            existingCountry.BlockedUntil = expirationTime;
            return true;
        }
        else
        {
            var country = new Country
            {
                Code = countryCode,
                Name = countryName,
                BlockedUntil = expirationTime,
                IsPermanentlyBlocked = false
            };

            return _blockedCountries.TryAdd(countryCode, country);
        }
    }

    public bool RemoveExpiredTemporalBlocks()
    {
        var currentTime = DateTime.UtcNow;
        var expiredBlocks = _blockedCountries.Values
            .Where(c => !c.IsPermanentlyBlocked && c.BlockedUntil.HasValue && c.BlockedUntil.Value <= currentTime)
            .ToList();

        foreach (var country in expiredBlocks)
        {
            _blockedCountries.TryRemove(country.Code, out _);
        }

        return expiredBlocks.Any();
    }

    public bool CountryExists(string countryCode)
    {
        return _blockedCountries.ContainsKey(countryCode);
    }
}