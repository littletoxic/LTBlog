namespace LTBlog.Test;

using LTBlog.Data;
using Microsoft.EntityFrameworkCore;

public class DatabaseTest(TestDatabaseFixture fixture) : IClassFixture<TestDatabaseFixture> {
    private readonly ArticleContext db = fixture.Context;

    [Fact]
    public void AddArticle() {
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
            db.SaveChanges();
        }
    }

    [Fact]
    public void AddArticleWithTag() {
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
            db.SaveChanges();
        }
    }

    [Fact]
    public void AddTag() {
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
            db.SaveChanges();
        }
    }
}

public sealed class TestDatabaseFixture : IDisposable {

    public TestDatabaseFixture() {
        var options = new DbContextOptionsBuilder<ArticleContext>()
            .UseNpgsql("Host=localhost;Database=test;Username=postgres;Password=&0k42^V2EOD*AjS")
            .UseSnakeCaseNamingConvention()
            .Options;
        Context = new(options);

        Context.Database.EnsureCreated();
    }

    public required ArticleContext Context {
        get; init;
    }

    public void Dispose() {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}
