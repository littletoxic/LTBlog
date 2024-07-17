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
            var article = db.Articles.FirstOrDefault(a => a.Id == articleId);
            Assert.NotNull(article);
            Assert.Equal("Test Article", article.Title);
            Assert.Equal("This is a test article.", article.Content);

            // Cleanup
            db.Articles.ExecuteDelete();
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
            var article = db.Articles.Include(a => a.Tags).FirstOrDefault();
            Assert.NotNull(article);
            Assert.Equal("Test Article", article.Title);
            Assert.Single(article.Tags);
            Assert.Equal(tagId, article.Tags[0].Id);

            // Cleanup
            db.Articles.ExecuteDelete();
            db.Tags.ExecuteDelete();
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
            var tag = db.Tags.FirstOrDefault(t => t.Id == tagId);
            Assert.NotNull(tag);
            Assert.Equal("Test Tag", tag.Name);

            // Cleanup
            db.Tags.ExecuteDelete();
        }
    }

    [Fact]
    public void ModifyUpdatedAt() {
        // arrange
        int articleId;
        using (var db = CreateContext()) {
            var article = new Article {
                Title = "Test Article",
                Tags = [],
                Content = "This is a test article."
            };
            db.Attach(article);
            db.SaveChanges();
            articleId = article.Id;
        }

        // act
        var updatedAt = DateTimeOffset.UtcNow;
        using (var db = CreateContext()) {
            var articleToUpdate = db.Articles.First(a => a.Id == articleId);
            articleToUpdate.UpdatedAt = updatedAt;
            articleToUpdate.Title = "Modified Article";
            db.SaveChanges();
        }

        // assert
        using (var db = CreateContext()) {
            var article = db.Articles.First(a => a.Id == articleId);
            Assert.Equal("Modified Article", article.Title);
            Assert.NotEqual(article.CreatedAt, article.UpdatedAt);

            // 允许一定的时间差异，以适应数据库和 .NET 的时间精度差异
            var timeDifference = Math.Abs((article.UpdatedAt - updatedAt).TotalMilliseconds);
            Assert.True(timeDifference < 1, $"Expected the time difference to be less than 1 millisecond, but got {timeDifference} milliseconds.");

            // Cleanup
            db.Articles.ExecuteDelete();
        }
    }

    private ArticleContext CreateContext() =>
        fixture.CreateContext(options =>
            options.LogTo(output.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information));
}
