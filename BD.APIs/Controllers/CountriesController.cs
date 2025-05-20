namespace BD.APIs.Controllers;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BD.Core.Interfaces.Repositories;
using BD.Core.Models;
using BD.Core.Models.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controller for managing blocked countries
/// </summary>
[Route("api/countries")]
[ApiController]
public class CountriesController : ControllerBase
{
    private readonly ICountryRepository _countryRepository;

    public CountriesController(ICountryRepository countryRepository)
    {
        _countryRepository = countryRepository;
    }

    /// <summary>
    /// Adds a country to the blocked list
    /// </summary>
    /// <param name="request">The country code to block (e.g., "US", "GB", "EG")</param>
    /// <returns>A confirmation message</returns>
    /// <response code="200">Country successfully blocked</response>
    /// <response code="400">Invalid country code or country already blocked</response>
    // POST /api/countries/block
    [HttpPost("block")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult BlockCountry([FromBody] BlockCountryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CountryCode) || request.CountryCode.Length != 2)
        {
            return BadRequest("Country code must be a valid 2-letter ISO code.");
        }

        var countryCode = request.CountryCode.ToUpper();
        
        var countryName = countryCode;

        var result = _countryRepository.AddBlockedCountry(countryCode, countryName);
        
        if (!result)
        {
            return BadRequest("Country is already blocked.");
        }

        return Ok(new { message = $"Country {countryCode} has been blocked." });
    }

    /// <summary>
    /// Removes a country from the blocked list
    /// </summary>
    /// <param name="countryCode">The country code to unblock (e.g., "US", "GB", "EG")</param>
    /// <returns>A confirmation message</returns>
    /// <response code="200">Country successfully unblocked</response>
    /// <response code="404">Country not found in blocked list</response>
    // DELETE /api/countries/block/{countryCode}
    [HttpDelete("block/{countryCode}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UnblockCountry(string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return BadRequest("Country code is required.");
        }

        countryCode = countryCode.ToUpper();

        var result = _countryRepository.RemoveBlockedCountry(countryCode);
        
        if (!result)
        {
            return NotFound($"Country {countryCode} is not blocked.");
        }

        return Ok(new { message = $"Country {countryCode} has been unblocked." });
    }

    /// <summary>
    /// Gets all blocked countries with pagination and search
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Results per page (default: 10)</param>
    /// <param name="search">Optional search term to filter countries by code or name</param>
    /// <returns>A list of blocked countries</returns>
    /// <response code="200">Returns the list of blocked countries</response>
    // GET /api/countries/blocked
    [HttpGet("blocked")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetBlockedCountries([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1 || pageSize > 100)
        {
            pageSize = 10;
        }

        var blockedCountries = _countryRepository.GetBlockedCountries(page, pageSize, search);
        
        return Ok(blockedCountries);
    }

    /// <summary>
    /// Temporarily blocks a country for a specific duration
    /// </summary>
    /// <param name="request">The request containing country code and block duration in minutes</param>
    /// <returns>A confirmation message with block expiration time</returns>
    /// <response code="200">Country successfully blocked temporarily</response>
    /// <response code="400">Invalid country code or duration</response>
    /// <response code="409">Country already blocked permanently or for a longer duration</response>
    // POST /api/countries/temporal-block
    [HttpPost("temporal-block")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public IActionResult TemporalBlockCountry([FromBody] TemporalBlockCountryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CountryCode) || request.CountryCode.Length != 2)
        {
            return BadRequest("Country code must be a valid 2-letter ISO code.");
        }

        if (request.DurationMinutes < 1 || request.DurationMinutes > 1440)
        {
            return BadRequest("Duration must be between 1 and 1440 minutes (24 hours).");
        }

        var countryCode = request.CountryCode.ToUpper();
        
        var countryName = countryCode;

        if (_countryRepository.IsCountryBlocked(countryCode) && 
            _countryRepository.GetBlockedCountries(searchTerm: countryCode)
                .Any(c => c.Code == countryCode && c.IsPermanentlyBlocked))
        {
            return Conflict($"Country {countryCode} is already permanently blocked.");
        }

        var result = _countryRepository.AddTemporalBlock(countryCode, countryName, request.DurationMinutes);
        
        if (!result)
        {
            return Conflict($"Country {countryCode} is already temporarily blocked for a longer duration.");
        }

        return Ok(new 
        { 
            message = $"Country {countryCode} has been temporarily blocked for {request.DurationMinutes} minutes.",
            expiresAt = DateTime.UtcNow.AddMinutes(request.DurationMinutes)
        });
    }
}