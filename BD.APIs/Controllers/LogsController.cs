namespace BD.APIs.Controllers;

using BD.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controller for accessing blocked access attempt logs
/// </summary>
[Route("api/logs")]
[ApiController]
public class LogsController : ControllerBase
{
    private readonly ILogsRepository _logsRepository;

    public LogsController(ILogsRepository logsRepository)
    {
        _logsRepository = logsRepository;
    }

    /// <summary>
    /// Gets a paginated list of blocked access attempts
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Results per page (default: 10)</param>
    /// <returns>A list of blocked attempt logs</returns>
    /// <response code="200">Returns the list of blocked attempt logs</response>
    [HttpGet("blocked-attempts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetBlockedAttempts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1 || pageSize > 100)
        {
            pageSize = 10;
        }

        var logs = _logsRepository.GetBlockedAttempts(page, pageSize);
        
        return Ok(logs);
    }
}