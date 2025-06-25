using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WTW.MdpService.Infrastructure.MdpDb;

namespace WTW.MdpService.Infrastructure.Db;

public class DatabaseConnectionParser : IDatabaseConnectionParser
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseConnectionParser> _logger;

    public DatabaseConnectionParser(string connectionString, ILogger<DatabaseConnectionParser> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public string GetSid()
    {
        var sidStart = _connectionString.IndexOf("(SID=", StringComparison.InvariantCultureIgnoreCase);
        _logger.LogInformation("Parsing SID from connection string");
        if (sidStart > -1)
        {
            sidStart += 5;
            var sidEnd = _connectionString.IndexOf(')', sidStart);
            var sid = _connectionString.Substring(sidStart, sidEnd - sidStart);
            
            _logger.LogInformation("SID extracted: {Sid}", sid);
            return sid;
        }
    
        _logger.LogWarning("Unable to find SID in the connection string");
        return null;
    }
}