using Forum.Domain.Exceptions;
using Forum.Domain.Models;
using Forum.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Forum.Application;

public sealed class CommentService(ForumDbContext dbContext) : ICommentService
{
    private readonly ForumDbContext _context = dbContext;

    public async Task DeleteById(long commentId)
    {
        var commentFromDb = await FindByIdAsync(commentId);
        _context.Comments.Remove(commentFromDb);

        if (await _context.SaveChangesAsync() < 1)
        {
            throw new ModifyFailedException("delete", "comment");
        }
    }

    public async Task<bool> ExistsByIdAsync(long commentId)
    {
        return await _context.Comments.AnyAsync(c => c.Id == commentId);
    }

    public async Task<Comment> FindByIdAsync(long commentId)
    {
        if (!await ExistsByIdAsync(commentId))
        {
            throw new EntityDoesNotExistException("comment", "ID", commentId);
        }

        return await _context.Comments
            .Include(c => c.Post)
            .Include(c => c.Replies)
            .FirstAsync(c => c.Id == commentId);
    }

    public async Task<List<Comment>> GetAllAsync()
    {
        return await _context.Comments
            .Include(c => c.Replies)
            .ToListAsync();
    }

    public async Task<List<Comment>> GetAllByAuthorId(long authorId)
    {
        return await _context.Comments
            .Where(c => c.AuthorId == authorId)
            .Include(c => c.Replies)
            .ToListAsync();
    }

    public async Task UpdateAsync(Comment updatedComment)
    {
        var commentFromDb = await FindByIdAsync(updatedComment.Id);
        commentFromDb.Body = updatedComment.Body;

        if (await _context.SaveChangesAsync() < 1)
        {
            throw new ModifyFailedException("update", "comment");
        }
    }

    public async Task<Comment> AddReply(long commentId, Comment reply)
    {
        var commentFromDb = await FindByIdAsync(commentId);
        reply.Post = commentFromDb.Post;
        commentFromDb.Replies.Add(reply);

        if (await _context.SaveChangesAsync() < 1)
        {
            throw new ModifyFailedException("save", "reply for comment");
        }

        return reply;
    }
}