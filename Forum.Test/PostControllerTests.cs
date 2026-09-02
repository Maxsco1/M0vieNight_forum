using Forum.Api.Controllers;
using Forum.Application;
using Forum.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace Forum.Test;

[TestFixture]
public class PostControllerTests
{
    private Mock<IPostService> _postServiceMock = null!;
    private PostController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _postServiceMock = new Mock<IPostService>(MockBehavior.Strict);
        _controller = new PostController(_postServiceMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _postServiceMock.Reset();
    }

    [Test]
    public async Task GetAllPostsReturnsOkWithAllPosts()
    {
        var posts = new List<Post>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            };

        _postServiceMock
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(posts);

        var result = await _controller.GetAllPosts();
        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.IsNotNull(okResult.Value);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreSame(posts, okResult.Value);
        _postServiceMock.Verify(s => s.GetAllAsync(), Times.Once);
        _postServiceMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GetAllPostsByAuthorIdReturnsOkWithPostsForAuthor()
    {
        long authorId = 42;
        var posts = new List<Post>
            {
                new() { Id = 10, AuthorId = authorId }
            };

        _postServiceMock
            .Setup(s => s.GetAllByAuthorId(authorId))
            .ReturnsAsync(posts);

        var result = await _controller.GetAllPostsByAuthorId(authorId);
        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.IsNotNull(okResult.Value);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreSame(posts, okResult.Value);

        _postServiceMock.Verify(s => s.GetAllByAuthorId(authorId), Times.Once);
        _postServiceMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GetPostByIdReturnsOkWithPostOnSuccess()
    {
        long postId = 5;
        var post = new Post { Id = postId };

        _postServiceMock
            .Setup(s => s.FindByIdAsync(postId))
            .ReturnsAsync(post);

        var result = await _controller.GetPostById(postId);
        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.IsNotNull(okResult.Value);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreSame(post, okResult.Value);

        _postServiceMock.Verify(s => s.FindByIdAsync(postId), Times.Once);
        _postServiceMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task CreatePostReturnsCreatedWithLocationAndCreatedEntityOnSuccess()
    {
        var input = new Post { Title = "New post" };
        var created = new Post { Id = 123, Title = input.Title };

        _postServiceMock
            .Setup(s => s.CreateAsync(input))
            .ReturnsAsync(created);

        var result = await _controller.CreatePost(input);
        var createdResult = result as CreatedResult;

        Assert.IsNotNull(createdResult);
        Assert.IsNotNull(createdResult.Value);
        Assert.AreEqual(201, createdResult.StatusCode);
        Assert.AreEqual($"api/posts/{created.Id}", createdResult.Location);
        Assert.AreSame(created, createdResult.Value);

        _postServiceMock.Verify(s => s.CreateAsync(input), Times.Once);
        _postServiceMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task AddCommentReturnsCreatedWithLocationAndCreatedCommentEntityOnSuccess()
    {
        long postId = 7;
        var commentInput = new Comment { Body = "Nice!" };
        var createdComment = new Comment { Id = 7, Body = commentInput.Body };

        _postServiceMock
            .Setup(s => s.AddComment(postId, commentInput))
            .ReturnsAsync(createdComment);

        var result = await _controller.AddComment(postId, commentInput);
        var createdResult = result as CreatedResult;

        Assert.IsNotNull(createdResult);
        Assert.IsNotNull(createdResult.Value);
        Assert.AreEqual(201, createdResult.StatusCode);
        Assert.AreEqual($"api/posts/{postId}/comments/{createdComment.Id}", createdResult.Location);
        Assert.AreEqual(createdComment, createdResult.Value);

        _postServiceMock.Verify(s => s.AddComment(postId, commentInput), Times.Once);
        _postServiceMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task UpdatePostReturnsOkWithUpdatedEntityOnSuccess()
    {
        var updatedPost = new Post { Id = 55, Title = "Updated" };

        _postServiceMock
            .Setup(s => s.UpdateAsync(updatedPost))
            .Returns(Task.CompletedTask);

        var result = await _controller.UpdatePost(updatedPost);
        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.IsNotNull(okResult.Value);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreSame(updatedPost, okResult.Value);

        _postServiceMock.Verify(s => s.UpdateAsync(updatedPost), Times.Once);
        _postServiceMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task DeletePostReturnsNoContentOnSuccess()
    {
        long postId = 88;

        _postServiceMock
            .Setup(s => s.DeleteById(postId))
            .Returns(Task.CompletedTask);

        var result = await _controller.DeletePost(postId);
        var noContentResult = result as NoContentResult;

        Assert.IsNotNull(noContentResult);
        Assert.AreEqual(204, noContentResult.StatusCode);

        _postServiceMock.Verify(s => s.DeleteById(postId), Times.Once);
        _postServiceMock.VerifyNoOtherCalls();
    }
}