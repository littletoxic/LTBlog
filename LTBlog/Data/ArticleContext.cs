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
        modelBuilder.Entity<Article>(a => {
            a.Property(a => a.CreatedAt)
                .HasDefaultValueSql("now()");
            a.Property(a => a.UpdatedAt)
                .HasDefaultValueSql("now()");
        });
    }
}
