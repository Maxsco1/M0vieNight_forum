using Forum.Application;
using Forum.Domain.Exceptions;
using Forum.Domain.Models;
using Forum.Infrastructure;
using NUnit.Framework;

namespace Forum.Test;

public class CommentServiceTests
{
    private readonly TestDatabaseContextFactory _factory = new();
    private ForumDbContext _forumDbContext = null!;
    private CommentService _commentService = null!;
    private Post _defaultPost = null!;

    [SetUp]
    public async Task Setup()
    {
        _forumDbContext = _factory.CreateContext();
        _commentService = new(_forumDbContext);
        _defaultPost = new(5, "BDAY INVITATION", "Please consider yourselves invited to my birthday party.", new List<Comment>(), new DateTime(2026, 8, 23, 20, 0, 0));

        await _forumDbContext.Posts.AddAsync(_defaultPost);
        await _forumDbContext.SaveChangesAsync();

        List<Comment> comments = [
            new Comment(1, "Won't anyone come to my birthday party?", _defaultPost, new DateTime(2026, 8, 24, 8, 0, 0)),
            new Comment(2, "I can't come.", _defaultPost, new DateTime(2026, 8, 24, 11, 25, 0)),
            new Comment(3, "Sure! I'll join.", _defaultPost, new DateTime(2026, 8, 24, 7, 30, 0)),
            new Comment(4, "The early bird gets the worm!", _defaultPost, new DateTime(2026, 8, 24, 5, 0, 0)),
            new Comment(1, "Anyone here?", _defaultPost, new DateTime(2026, 8, 24, 8, 49, 12))
        ];

        comments.First().Replies = [comments.Last()];

        // Without skipping the final element, the last comment will be added twice because 
        // of the Replies assignment above, causing a conflict.
        foreach (var comment in comments.SkipLast(1))
        {
            await _forumDbContext.Comments.AddAsync(comment);
        }

        await _forumDbContext.SaveChangesAsync();
    }

    [TearDown]
    public async Task Teardown()
    {
        _forumDbContext.Dispose();
        _factory.Dispose();
    }

    [Test]
    public void DeleteCommentByIdThrowsEntityDoesNotExistExceptionWhenCommentDoesNotExist()
    {
        Assert.ThrowsAsync<EntityDoesNotExistException>(() => _commentService.DeleteById(-1));
    }

    [Test]
    public async Task DeleteCommentByIdDeletesEntityWhenValid()
    {
        var toDeleteId = 2;

        await _commentService.DeleteById(toDeleteId);

        Assert.ThrowsAsync<EntityDoesNotExistException>(() => _commentService.FindByIdAsync(toDeleteId));
    }

    [Test]
    public async Task ExistsByIdAsyncReturnsTrueForExistingId()
    {
        Assert.True(await _commentService.ExistsByIdAsync(1));
    }

    [Test]
    public async Task ExistsByIdAsyncReturnsFalseForNonExistentId()
    {
        Assert.False(await _commentService.ExistsByIdAsync(-1));
    }

    [Test]
    public void FindByIdAsyncThrowsEntityDoesNotExistExceptionWhenCommentDoesNotExist()
    {
        Assert.ThrowsAsync<EntityDoesNotExistException>(() => _commentService.FindByIdAsync(-1));
    }

    [Test]
    public async Task FindByIdAsyncReturnsEntityWhenValid()
    {
        var found = await _commentService.FindByIdAsync(3);
        Assert.AreEqual(3, found.Id);
        Assert.AreEqual(3, found.AuthorId);
        Assert.AreEqual("Sure! I'll join.", found.Body);
    }

    [Test]
    public async Task GetAllCommentsAsyncReturnsAllComments()
    {
        var all = await _commentService.GetAllAsync();
        Assert.AreEqual(all.Select(x => x.Id).ToArray(), new[] { 1L, 2L, 3L, 4L, 5L });
    }

    [Test]
    public async Task GetAllCommentsByAuthorIdReturnsOnlyCommentsForThatAuthorId()
    {
        var result = await _commentService.GetAllByAuthorId(1);
        Assert.That(result, Has.Exactly(1).Matches<Comment>(c => c.Id == 5 && c.AuthorId == 1));
    }

    [Test]
    public async Task GetAllCommentsByAuthorIdReturnsEmptyListWhenNoMatches()
    {
        var result = await _commentService.GetAllByAuthorId(-1);
        Assert.IsEmpty(result);
    }

    [Test]
    public void UpdateAsyncThrowsEntityDoesNotExistExceptionWhenCommentDoesNotExist()
    {
        var updated = new Comment(-1, "doesn't matter", _defaultPost, DateTime.UtcNow);

        Assert.ThrowsAsync<EntityDoesNotExistException>(() => _commentService.UpdateAsync(updated));
    }

    [Test]
    public async Task UpdateAsyncUpdatesBodyWhenValid()
    {
        var updated = new Comment(1, "UPDATED BODY", _defaultPost, DateTime.UtcNow) { Id = 1 };

        await _commentService.UpdateAsync(updated);

        var fromDb = await _commentService.FindByIdAsync(1);
        Assert.AreEqual("UPDATED BODY", fromDb.Body);
    }

    [Test]
    public async Task AddReplyThrowsIfParentCommentDoesNotExist()
    {
        var replyWithNonExistentParent = new Comment(1, "reply", _defaultPost, DateTime.UtcNow);
        Assert.ThrowsAsync<EntityDoesNotExistException>(() => _commentService.AddReply(-1, replyWithNonExistentParent));
    }

    [Test]
    public async Task AddReplyReturnsReplyIfParentExists()
    {
        var replyWithParent = new Comment(1, "Reply", _defaultPost, DateTime.UtcNow);

        var commentFromDbBefore = await _commentService.FindByIdAsync(3);
        Assert.False(commentFromDbBefore.Replies.Contains(replyWithParent));

        await _commentService.AddReply(3, replyWithParent);

        var commentFromDbAfter = await _commentService.FindByIdAsync(3);
        Assert.True(commentFromDbAfter.Replies.Contains(replyWithParent));
    }
}