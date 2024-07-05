namespace LTBlog.Test;

using LTBlog.Data;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

public class DatabaseTest(TestDatabaseFixture fixture, ITestOutputHelper output) : IClassFixture<TestDatabaseFixture> {
    private readonly TestDatabaseFixture fixture = fixture;

    private readonly ITestOutputHelper output = output;

    [Fact]
    public void AddArticle() {
        // arrange
        int articleId;
        using (var db = CreateContext()) {
            var article = new Article {
                Title = "Test Article",
                Tags = [],
                Content = "This is a test article."
            };
            db.Articles.Add(article);
            db.SaveChanges();
            articleId = article.Id;
        }

        // assert
        using (var db = CreateContext()) {
            try {
                var article = db.Articles.FirstOrDefault(a => a.Id == articleId);
                Assert.NotNull(article);
                Assert.Equal("Test Article", article.Title);
                Assert.Equal("This is a test article.", article.Content);
            } finally {
                // Cleanup
                db.Articles.ExecuteDelete();
            }
        }
    }

    [Fact]
    public void AddArticleWithExistTag() {
        // arrange
        int tagId;
        using (var db = CreateContext()) {
            var tag = new Tag {
                Name = "Test Tag",
                Articles = []
            };
            db.Tags.Add(tag);
            db.SaveChanges();
            tagId = tag.Id;
        }

        // act
        using (var db = CreateContext()) {
            var article = new Article {
                Title = "Test Article",
                Tags = [.. db.Tags.Where(t => t.Id == tagId)],
                Content = "This is a test article."
            };
            db.Articles.Add(article);
            db.SaveChanges();
        }

        // assert
        using (var db = CreateContext()) {
            try {
                var article = db.Articles.Include(a => a.Tags).FirstOrDefault();
                Assert.NotNull(article);
                Assert.Equal("Test Article", article.Title);
                Assert.Single(article.Tags);
                Assert.Equal(tagId, article.Tags[0].Id);
            } finally {
                db.Articles.ExecuteDelete();
                db.Tags.ExecuteDelete();
            }
        }
    }

    [Fact]
    public void AddTag() {
        // arrange
        int tagId;
        using (var db = CreateContext()) {
            var tag = new Tag {
                Name = "Test Tag",
                Articles = []
            };
            db.Tags.Add(tag);
            db.SaveChanges();
            tagId = tag.Id;
        }

        // assert
        using (var db = CreateContext()) {
            try {
                var tag = db.Tags.FirstOrDefault(t => t.Id == tagId);
                Assert.NotNull(tag);
                Assert.Equal("Test Tag", tag.Name);
            } finally {
                // Cleanup
                db.Tags.ExecuteDelete();
            }
        }
    }

    [Fact]
    public void ModifyUpdatedAt() {
        // arrange
        using (var db = CreateContext()) {
            var article = new Article {
                Title = "Test Article",
                Tags = [],
                Content = "This is a test article."
            };
            db.Attach(article);
            db.SaveChanges();
        }

        // act
        var updatedAt = DateTimeOffset.UtcNow;
        using (var db = CreateContext()) {
            var articleToUpdate = db.Articles.First();
            articleToUpdate.UpdatedAt = updatedAt;
            articleToUpdate.Title = "Modified Article";
            db.SaveChanges();
        }

        // assert
        using (var db = CreateContext()) {
            var article = db.Articles.First();
            try {
                Assert.Equal("Modified Article", article.Title);
                Assert.Equal(updatedAt, article.UpdatedAt);
                Assert.NotEqual(article.CreatedAt, article.UpdatedAt);
            } finally {
                db.Articles.ExecuteDelete();
            }
        }
    }

    private ArticleContext CreateContext() =>
        fixture.CreateContext(options =>
        options.LogTo(output.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information));
}

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
