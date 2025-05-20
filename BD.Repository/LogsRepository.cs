namespace BD.Repository;

using System.Collections.Concurrent;
using BD.Core.Interfaces.Repositories;
using BD.Core.Models;

public class LogsRepository : ILogsRepository
{
    private readonly ConcurrentBag<BlockedAttemptLog> _logs = new();

    public void LogBlockedAttempt(BlockedAttemptLog log)
    {
        _logs.Add(log);
    }

    public List<BlockedAttemptLog> GetBlockedAttempts(int page = 1, int pageSize = 10)
    {
        return _logs
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }
}