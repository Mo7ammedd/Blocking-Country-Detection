namespace BD.Core.Interfaces.Repositories;

using BD.Core.Models;

public interface ILogsRepository
{
    void LogBlockedAttempt(BlockedAttemptLog log);
    List<BlockedAttemptLog> GetBlockedAttempts(int page = 1, int pageSize = 10);
}