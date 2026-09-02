namespace Forum.Application;

using Forum.Domain.Models;

public interface IPostService
{
    Task<Post> FindByIdAsync(long postId);
    Task<List<Post>> GetAllAsync();
    Task<List<Post>> GetAllByAuthorId(long authorId);
    Task<bool> ExistsByIdAsync(long postId);
    Task<Post> CreateAsync(Post post);
    Task UpdateAsync(Post updatedPost);
    Task DeleteById(long postId);
    Task<Comment> AddComment(long postId, Comment comment);
}