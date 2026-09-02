namespace Forum.Domain.Models;

public class Post
{
    public long Id { get; set; }
    public long AuthorId { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public List<Comment> Comments { get; set; } = [];
    public DateTime PostedOn { get; set; }

    public Post()
    {
        Title = "";
        Body = "";
    }

    public Post(long authorId, string title, string body, List<Comment> comments, DateTime postedOn)
    {
        AuthorId = authorId;
        Title = title;
        Body = body;
        Comments = comments;
        PostedOn = postedOn;
    }
}