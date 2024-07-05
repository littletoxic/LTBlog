namespace LTBlog.Data;

public class Article {

    public required string Content {
        get; set;
    }

    public DateTimeOffset CreatedAt {
        get; set;
    }

    public int Id {
        get; init;
    }

    public virtual required List<Tag> Tags {
        get; set;
    }

    public required string Title {
        get; set;
    }

    public DateTimeOffset UpdatedAt {
        get; set;
    }
}
