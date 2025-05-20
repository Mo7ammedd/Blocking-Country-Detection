namespace BD.APIs.Controllers;

using System.Threading.Tasks;
using BD.Core.Interfaces.Repositories;
using BD.Core.Interfaces.Services;
using BD.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controller for IP address validation and blocking checks
/// </summary>
[Route("api/ip")]
[ApiController]
public class IpController : ControllerBase
{
    private readonly IIpGeolocationService _ipGeolocationService;
    private readonly ICountryRepository _countryRepository;
    private readonly ILogsRepository _logsRepository;

    public IpController(
        IIpGeolocationService ipGeolocationService,
        ICountryRepository countryRepository,
        ILogsRepository logsRepository)
    {
        _ipGeolocationService = ipGeolocationService;
        _countryRepository = countryRepository;
        _logsRepository = logsRepository;
    }

    /// <summary>
    /// Looks up geolocation information for an IP address
    /// </summary>
    /// <param name="ipAddress">Optional IP address to look up. If not provided, the caller's IP will be used</param>
    /// <returns>Geolocation data for the IP address</returns>
    /// <response code="200">Returns the geolocation data</response>
    /// <response code="400">Invalid IP address format</response>
    /// <response code="404">Geolocation data not found for the IP address</response>
    [HttpGet("lookup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LookupIp([FromQuery] string? ipAddress = null)
    {
        string ip = ipAddress ?? GetCallerIp();

        if (string.IsNullOrWhiteSpace(ip))
        {
            return BadRequest("Invalid IP address format.");
        }

        var geolocationData = await _ipGeolocationService.GetIpGeolocationDataAsync(ip);

        if (geolocationData == null)
        {
            return NotFound($"Could not find geolocation data for IP address: {ip}");
        }

        return Ok(geolocationData);
    }

    /// <summary>
    /// Checks if the caller's IP address is from a blocked country
    /// </summary>
    /// <returns>IP verification result with blocking status</returns>
    /// <response code="200">Returns the verification result</response>
    [HttpGet("check-block")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckIpBlock()
    {
        string ip = GetCallerIp();
        bool isBlocked = false;
        string countryCode = string.Empty;
        string userAgent = Request.Headers.UserAgent.ToString();

        var geolocationData = await _ipGeolocationService.GetIpGeolocationDataAsync(ip);

        if (geolocationData != null)
        {
            countryCode = geolocationData.CountryCode;
            
            isBlocked = _countryRepository.IsCountryBlocked(countryCode);
        }

        _logsRepository.LogBlockedAttempt(new BlockedAttemptLog
        {
            Ip = ip,
            CountryCode = countryCode,
            IsBlocked = isBlocked,
            UserAgent = userAgent,
            Timestamp = DateTime.UtcNow
        });

        return Ok(new
        {
            ip,
            countryCode,
            isBlocked,
            message = isBlocked 
                ? $"Access blocked. Your country ({countryCode}) is in the blocked list." 
                : "Access allowed."
        });
    }

    private string GetCallerIp()
    {
        var forwardedIp = Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();

        return forwardedIp ?? HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
    }
}