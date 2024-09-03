namespace LTBlog.Data;

public class Tag {

    public virtual required List<Article> Articles {
        get; set;
    }

    public int Id {
        get; init;
    }

    public required string Name {
        get; set;
    }
}
