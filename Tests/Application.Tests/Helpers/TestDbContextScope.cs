using Application.Common.Interfaces;
using Domain.Comments;
using Domain.Users;
using Domain.Videos;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Application.Tests.Helpers;

public sealed class TestApplicationDbContext : DbContext, IApplicationDbContext
{
    public TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<HighlightVideo> Videos => Set<HighlightVideo>();
    public DbSet<VideoLike> VideoLikes => Set<VideoLike>();
    public DbSet<Comment> Comments => Set<Comment>();
}

public sealed class TestDbContextScope : IDisposable
{
    public SqliteConnection Connection { get; }
    public TestApplicationDbContext Context { get; }

    private TestDbContextScope(SqliteConnection connection, TestApplicationDbContext context)
    {
        Connection = connection;
        Context = context;
    }

    public static TestDbContextScope Create()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new TestApplicationDbContext(options);
        context.Database.EnsureCreated();

        return new TestDbContextScope(connection, context);
    }

    public void Dispose()
    {
        Context.Dispose();
        Connection.Dispose();
    }
}
