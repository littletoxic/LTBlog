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
        }
    }

    [Fact]
    public void AddTagWithArticle() {
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

public sealed class TestDatabaseFixture : IDisposable {

    public TestDatabaseFixture() {
        var options = new DbContextOptionsBuilder<ArticleContext>()
            .UseNpgsql("Host=localhost;Database=test;Username=postgres;Password=&0k42^V2EOD*AjS;Include Error Detail=true")
            .UseSnakeCaseNamingConvention()
            .Options;
        Context = new(options);

        Context.Database.EnsureDeleted();
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
