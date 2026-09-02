namespace Forum.Application;

using Forum.Domain.Models;

public interface ICommentService
{
    Task<List<Comment>> GetAllAsync();
    Task<List<Comment>> GetAllByAuthorId(long authorId);
    Task<bool> ExistsByIdAsync(long commentId);
    Task<Comment> FindByIdAsync(long commentId);
    Task UpdateAsync(Comment updatedComment);
    Task DeleteById(long commentId);
    Task<Comment> AddReply(long commentId, Comment comment);
}