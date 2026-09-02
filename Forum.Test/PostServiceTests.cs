using Forum.Application;
using Forum.Domain.Exceptions;
using Forum.Domain.Models;
using Forum.Infrastructure;
using NUnit.Framework;

namespace Forum.Test;

public class PostServiceTests
{
    private readonly TestDatabaseContextFactory _factory = new();
    private ForumDbContext _forumDbContext = null!;
    private PostService _postService = null!;
    private Post _defaultPost = null!;

    [SetUp]
    public async Task Setup()
    {
        _forumDbContext = _factory.CreateContext();
        _postService = new(_forumDbContext);
        _defaultPost = new(5, "BDAY INVITATION", "Please consider yourselves invited to my birthday party.", new List<Comment>(), new DateTime(2026, 8, 23, 20, 0, 0));

        await _postService.CreateAsync(_defaultPost);

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
            await _postService.AddComment(_defaultPost.Id, comment);
        }
    }

    [TearDown]
    public async Task Teardown()
    {
        _forumDbContext.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task CreateAsyncThrowsEntityAlreadyExistsExceptionWhenIdAlreadyExists()
    {
        var existing = new Post(1, "Help me move!", "Hi! I need some help moving. I'm flat broke and cannot afford to rent a truck. I sure hope my \"friends\" will step in and help me.", [], DateTime.UtcNow)
        {
            Id = _defaultPost.Id
        };

        Assert.ThrowsAsync<EntityAlreadyExistsException>(async () => await _postService.CreateAsync(existing));
    }

    [Test]
    public async Task CreateAsyncReturnsCreatedEntityAndPersistsWhenValid()
    {
        var post = new Post(1, "Example!", "Example.", [], DateTime.UtcNow);

        var created = await _postService.CreateAsync(post);

        Assert.AreEqual(post.AuthorId, created.AuthorId);
        Assert.AreEqual(post.Id, created.Id);
        Assert.AreEqual(post.Title, created.Title);
        Assert.AreEqual(post.Body, created.Body);

        var fromDb = await _postService.FindByIdAsync(post.Id);
        Assert.AreEqual(created.Body, fromDb.Body);
    }

    [Test]
    public void DeletePostByIdThrowsEntityDoesNotExistExceptionWhenCommentDoesNotExist()
    {
        Assert.ThrowsAsync<EntityDoesNotExistException>(() => _postService.DeleteById(-1));
    }

    [Test]
    public async Task DeletePostByIdDeletesEntityWhenValid()
    {
        var toDeleteId = _defaultPost.Id;

        await _postService.DeleteById(toDeleteId);

        Assert.ThrowsAsync<EntityDoesNotExistException>(() => _postService.FindByIdAsync(toDeleteId));
    }

    [Test]
    public async Task ExistsByIdAsyncReturnsTrueForExistingId()
    {
        Assert.True(await _postService.ExistsByIdAsync(1));
    }

    [Test]
    public async Task ExistsByIdAsyncReturnsFalseForNonExistentId()
    {
        Assert.False(await _postService.ExistsByIdAsync(-1));
    }

    [Test]
    public void FindByIdAsyncThrowsEntityDoesNotExistExceptionWhenCommentDoesNotExist()
    {
        Assert.ThrowsAsync<EntityDoesNotExistException>(() => _postService.FindByIdAsync(-1));
    }

    [Test]
    public async Task FindByIdAsyncReturnsEntityWhenValid()
    {
        var found = await _postService.FindByIdAsync(_defaultPost.Id);
        Assert.AreEqual(_defaultPost.Id, found.Id);
        Assert.AreEqual(5, found.AuthorId);
        Assert.AreEqual("BDAY INVITATION", found.Title);
    }

    [Test]
    public async Task GetAllPostsAsyncReturnsAllPosts()
    {
        var all = await _postService.GetAllAsync();
        Assert.AreEqual(all.Select(x => x.Id).ToArray(), new[] { _defaultPost.Id });
    }

    [Test]
    public async Task GetPostsByAuthorIdReturnsOnlyPostsForThatAuthorId()
    {
        var result = await _postService.GetAllByAuthorId(_defaultPost.AuthorId);
        Assert.That(result, Has.Exactly(1).Matches<Post>(p => p.Id == _defaultPost.Id && p.AuthorId == _defaultPost.AuthorId));
    }

    [Test]
    public async Task GetAllCommentsByAuthorIdReturnsEmptyListWhenNoMatches()
    {
        var result = await _postService.GetAllByAuthorId(-1);
        Assert.IsEmpty(result);
    }

    [Test]
    public void UpdateAsyncThrowsEntityDoesNotExistExceptionWhenPostDoesNotExist()
    {
        var updatedPost = new Post(1, "Example!", "Example.", [], DateTime.UtcNow);

        Assert.ThrowsAsync<EntityDoesNotExistException>(() => _postService.UpdateAsync(updatedPost));
    }

    [Test]
    public async Task UpdateAsyncUpdatesBodyWhenValid()
    {
        var post = new Post(1, "Example!", "Example.", [], DateTime.UtcNow) { Id = _defaultPost.Id };

        await _postService.UpdateAsync(post);

        var fromDb = await _postService.FindByIdAsync(1);
        Assert.AreEqual("Example!", fromDb.Title);
    }

    [Test]
    public async Task AddCommentThrowsIfNoPostWithIdExists()
    {
        var replyWithNonExistentParent = new Comment(1, "comment", _defaultPost, DateTime.UtcNow);
        Assert.ThrowsAsync<EntityDoesNotExistException>(() => _postService.AddComment(-1, replyWithNonExistentParent));
    }

    [Test]
    public async Task AddCommentReturnsCommentIfPostWithIdExists()
    {
        var commentWithParent = new Comment(1, "Reply", _defaultPost, DateTime.UtcNow);

        var postFromDbBefore = await _postService.FindByIdAsync(_defaultPost.Id);
        Assert.False(postFromDbBefore.Comments.Contains(commentWithParent));

        await _postService.AddComment(_defaultPost.Id, commentWithParent);

        var postFromDbAfter = await _postService.FindByIdAsync(_defaultPost.Id);
        Assert.True(postFromDbAfter.Comments.Contains(commentWithParent));
    }
}
