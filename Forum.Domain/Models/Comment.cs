using System.Text.Json.Serialization;

namespace Forum.Domain.Models;

public sealed class Comment
{
    public long Id { get; set; } = 0L;
    public long AuthorId { get; set; }
    public string Body { get; set; }
    [JsonIgnore]
    public Post? Post { get; set; } = null;
    public List<Comment> Replies { get; set; } = [];
    public DateTime PostedOn { get; set; }

    public Comment()
    {
        Body = "";
    }

    public Comment(long authorId, string body, Post post, DateTime postedOn)
    {
        AuthorId = authorId;
        Body = body;
        Post = post;
        PostedOn = postedOn;
    }
}