using Infrastructure.Persistence.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Tests.Helpers;

public sealed class TestDbContextScope : IDisposable
{
    public SqliteConnection Connection { get; }
    public AppDbContext Context { get; }

    private TestDbContextScope(SqliteConnection connection, AppDbContext context)
    {
        Connection = connection;
        Context = context;
    }

    public static TestDbContextScope Create()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);

        return new TestDbContextScope(connection, context);
    }

    public void Dispose()
    {
        Context.Dispose();
        Connection.Dispose();
    }
}
