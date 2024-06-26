namespace LTBlog.Test;

using LTBlog.Data;
using Microsoft.EntityFrameworkCore;

public class DatabaseTest(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture> {

    public required TestDatabaseFixture Fixture {
        get; init;
    } = fixture;

    [Fact]
    public void AddArticle() {
        using var db = Fixture.CreateContext();
        var article = new Article {
            Title = "Test Article",
            Tags = [],
            Content = "This is a test article."
        };
        db.Attach(article);
        db.SaveChanges();

        try {
            Assert.Equal(1, db.Articles.Count());
        } finally {
            db.Articles.ExecuteDelete();
        }
    }

    [Fact]
    public void AddArticleWithTag() {
        using var db = Fixture.CreateContext();
        var tag = new Tag {
            Name = "Test Tag",
            Articles = []
        };
        db.Attach(tag);
        db.SaveChanges();

        var article = new Article {
            Title = "Test Article",
            Tags = [tag],
            Content = "This is a test article."
        };
        db.Attach(article);
        db.SaveChanges();

        try {
            Assert.Equal(1, db.Articles.Count());
            Assert.Equal(1, db.Tags.Count());
        } finally {
            db.Articles.ExecuteDelete();
            db.Tags.ExecuteDelete();
        }
    }

    [Fact]
    public void AddTag() {
        using var db = Fixture.CreateContext();
        var tag = new Tag {
            Name = "Test Tag",
            Articles = []
        };
        db.Attach(tag);
        db.SaveChanges();

        try {
            Assert.Equal(1, db.Tags.Count());
        } finally {
            db.Tags.ExecuteDelete();
        }
    }

    [Fact]
    public void AddTagWithArticle() {
        using var db = Fixture.CreateContext();
        var article = new Article {
            Title = "Test Article",
            Tags = [],
            Content = "This is a test article."
        };
        db.Attach(article);
        db.SaveChanges();

        var tag = new Tag {
            Name = "Test Tag",
            Articles = [article]
        };
        db.Attach(tag);
        db.SaveChanges();

        try {
            Assert.Equal(1, db.Articles.Count());
            Assert.Equal(1, db.Tags.Count());
        } finally {
            db.Articles.ExecuteDelete();
            db.Tags.ExecuteDelete();
        }
    }

    [Fact]
    public void ArticleWithExistTag() {
        using var db = Fixture.CreateContext();
        var tag = new Tag {
            Name = "Test Tag",
            Articles = []
        };
        db.Attach(tag);
        db.SaveChanges();

        var article = new Article {
            Title = "Test Article",
            Tags = [.. db.Tags.Where(t => t.Name == "Test Tag")],
            Content = "This is a test article."
        };
        db.Attach(article);
        db.SaveChanges();

        try {
            Assert.Equal(1, db.Articles.Count());
            Assert.Equal(1, db.Tags.Count());
        } finally {
            db.Articles.ExecuteDelete();
            db.Tags.ExecuteDelete();
        }
    }
}

public sealed class TestDatabaseFixture {
    private const string ConnectionString = "Host=localhost;Database=test;Username=postgres;Password=&0k42^V2EOD*AjS;Include Error Detail=true";
    private static readonly object _lock = new();
    private static bool _databaseInitialized;

    private readonly DbContextOptions<ArticleContext> options =
        new DbContextOptionsBuilder<ArticleContext>()
            .UseNpgsql(ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

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

    public ArticleContext CreateContext() =>
        new(options);
}
