using Forum.Domain.Exceptions;
using Forum.Domain.Models;
using Forum.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Forum.Application;

public sealed class PostService(ForumDbContext context) : IPostService
{
    private readonly ForumDbContext _context = context;

    public async Task<Post> CreateAsync(Post post)
    {
        if (await ExistsByIdAsync(post.Id))
        {
            throw new EntityAlreadyExistsException("post", "ID", post.Id);
        }

        post.PostedOn = DateTime.Now;
        await _context.Posts.AddAsync(post);

        if (await _context.SaveChangesAsync() < 1)
        {
            throw new ModifyFailedException("save", "post");
        }

        return post;
    }

    public async Task DeleteById(long postId)
    {
        var postFromDb = await FindByIdAsync(postId);
        _context.Posts.Remove(postFromDb);

        if (await _context.SaveChangesAsync() < 1)
        {
            throw new ModifyFailedException("delete", "post");
        }
    }

    public async Task<bool> ExistsByIdAsync(long postId)
    {
        return await _context.Posts.AnyAsync(p => p.Id == postId);
    }

    public async Task<Post> FindByIdAsync(long postId)
    {
        if (!await ExistsByIdAsync(postId))
        {
            throw new EntityDoesNotExistException("post", "ID", postId);
        }

        return await _context.Posts
            .Include(p => p.Comments)
            .FirstAsync(p => p.Id == postId);
    }

    public async Task<List<Post>> GetAllAsync()
    {
        return await _context.Posts
            .Include(p => p.Comments)
            .ToListAsync();
    }

    public async Task<List<Post>> GetAllByAuthorId(long authorId)
    {
        return await _context.Posts
            .Include(p => p.Comments)
            .Where(p => p.AuthorId == authorId)
            .ToListAsync();
    }

    public async Task UpdateAsync(Post updatedPost)
    {
        var postFromDb = await FindByIdAsync(updatedPost.Id);
        postFromDb.Title = updatedPost.Title;
        postFromDb.Body = updatedPost.Body;
        _context.Update(postFromDb);

        if (await _context.SaveChangesAsync() < 1)
        {
            throw new ModifyFailedException("update", "post");
        }
    }

    public async Task<Comment> AddComment(long postId, Comment comment)
    {
        var postFromDb = await FindByIdAsync(postId);
        comment.Post = postFromDb;
        comment.PostedOn = DateTime.Now;
        postFromDb.Comments.Add(comment);

        if (await _context.SaveChangesAsync() < 1)
        {
            throw new ModifyFailedException("save", "comment for post");
        }

        return comment;
    }
}