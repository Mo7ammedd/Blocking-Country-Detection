namespace BD.Services;

using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BD.Core.Interfaces.Services;
using BD.Core.Models;
using BD.Services.Config;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

public class IpGeolocationService : IIpGeolocationService
{
    private readonly HttpClient _httpClient;
    private readonly GeoLocationApiConfig _config;
    private static readonly Regex IpRegex = new Regex(@"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$", RegexOptions.Compiled);

    public IpGeolocationService(HttpClient httpClient, IOptions<GeoLocationApiConfig> config)
    {
        _httpClient = httpClient;
        _config = config.Value;
    }

    public async Task<IpGeolocationData?> GetIpGeolocationDataAsync(string ipAddress)
    {
        if (!IsValidIpAddress(ipAddress))
        {
            return null;
        }

        try
        {
            var requestUrl = $"{_config.ApiUrl}?apiKey={_config.ApiKey}&ip={ipAddress}";
            
            var response = await _httpClient.GetAsync(requestUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var geoData = JsonConvert.DeserializeObject<dynamic>(jsonContent);

            if (geoData == null)
            {
                return null;
            }

            return new IpGeolocationData
            {
                Ip = geoData.ip?.ToString() ?? ipAddress,
                CountryCode = geoData.country_code2?.ToString() ?? string.Empty,
                CountryName = geoData.country_name?.ToString() ?? string.Empty,
                City = geoData.city?.ToString() ?? string.Empty,
                Region = geoData.state_prov?.ToString() ?? string.Empty,
                Isp = geoData.isp?.ToString() ?? string.Empty
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting geolocation data: {ex.Message}");
            return null;
        }
    }

    private static bool IsValidIpAddress(string ipAddress)
    {
        return !string.IsNullOrWhiteSpace(ipAddress) && IpRegex.IsMatch(ipAddress);
    }
}

