namespace LTBlog.Test;

using LTBlog.Data;
using Microsoft.EntityFrameworkCore;

public sealed class TestDatabaseFixture {

    private const string ConnectionString =
        "Host=localhost;Database=test;Username=postgres;Password=&0k42^V2EOD*AjS;Include Error Detail=true";

    private static readonly object _lock = new();
    private static bool _databaseInitialized;

    private readonly DbContextOptionsBuilder<ArticleContext> options =
        new DbContextOptionsBuilder<ArticleContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention();

    public TestDatabaseFixture() {
        lock (_lock) {
            if (!_databaseInitialized) {
                using var db = CreateContext();

                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                _databaseInitialized = true;
            }
        }
    }

    public ArticleContext CreateContext(Func<DbContextOptionsBuilder<ArticleContext>, DbContextOptionsBuilder<ArticleContext>> func) =>
        new(func(options).Options);

    public ArticleContext CreateContext() => new(options.Options);
}
