namespace LTBlog.Data;

using Microsoft.EntityFrameworkCore;

public class ArticleContext(DbContextOptions<ArticleContext> options) : DbContext(options) {

    public DbSet<Article> Articles {
        get; set;
    }

    public DbSet<Tag> Tags {
        get; set;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Article>(builder => {
            builder.Property(a => a.CreatedAt)
                .HasDefaultValueSql("now()");
            builder.Property(a => a.UpdatedAt)
                .HasDefaultValueSql("now()");
        });
    }
}
