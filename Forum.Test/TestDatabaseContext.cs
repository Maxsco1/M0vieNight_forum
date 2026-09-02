using System.Data.Common;
using Forum.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Forum.Test;

public class TestDatabaseContextFactory : IDisposable
{
    private readonly List<DbConnection> dbConnections = new();

    private DbContextOptions<ForumDbContext> CreateOptions(DbConnection connection)
    {
        return new DbContextOptionsBuilder<ForumDbContext>()
            .UseSqlite(connection)
            .Options;
    }

    public ForumDbContext CreateContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        dbConnections.Add(connection);

        var options = CreateOptions(connection);
        using (var init = new ForumDbContext(options))
        {
            init.Database.EnsureCreated();
        }

        return new ForumDbContext(options);
    }

    public void Dispose()
    {
        foreach (var c in dbConnections)
        {
            try { c.Dispose(); } catch { }
        }

        dbConnections.Clear();
    }
}