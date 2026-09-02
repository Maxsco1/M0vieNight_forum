using Forum.Api.Controllers;
using Forum.Application;
using Forum.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace Forum.Test;

[TestFixture]
public class CommentControllerTests
{
    private Mock<ICommentService> _commentServiceMock = null!;
    private CommentController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _commentServiceMock = new Mock<ICommentService>(MockBehavior.Strict);
        _controller = new CommentController(_commentServiceMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _commentServiceMock.Reset();
    }

    [Test]
    public async Task GetAllCommentsReturnsOkWithAllComments()
    {
        var posts = new List<Comment>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            };

        _commentServiceMock
            .Setup(s => s.GetAllAsync())
            .ReturnsAsync(posts);

        var result = await _controller.GetAllComments();
        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.IsNotNull(okResult.Value);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreSame(posts, okResult.Value);
        _commentServiceMock.Verify(s => s.GetAllAsync(), Times.Once);
        _commentServiceMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GetAllCommentsByAuthorIdReturnsOkWithCommentsForAuthor()
    {
        long authorId = 42;
        var posts = new List<Comment>
            {
                new() { Id = 10, AuthorId = authorId }
            };

        _commentServiceMock
            .Setup(s => s.GetAllByAuthorId(authorId))
            .ReturnsAsync(posts);

        var result = await _controller.GetAllCommentsByAuthorId(authorId);
        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.IsNotNull(okResult.Value);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreSame(posts, okResult.Value);

        _commentServiceMock.Verify(s => s.GetAllByAuthorId(authorId), Times.Once);
        _commentServiceMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GetCommentByIdReturnsOkWithCommentOnSuccess()
    {
        long commentId = 5;
        var post = new Comment { Id = commentId };

        _commentServiceMock
            .Setup(s => s.FindByIdAsync(commentId))
            .ReturnsAsync(post);

        var result = await _controller.GetCommentById(commentId);
        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.IsNotNull(okResult.Value);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreSame(post, okResult.Value);

        _commentServiceMock.Verify(s => s.FindByIdAsync(commentId), Times.Once);
        _commentServiceMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task AddReplyReturnsCreatedWithLocationAndCreatedCommentEntityOnSuccess()
    {
        long parentCommentId = 7;
        var commentInput = new Comment { Body = "Nice!" };
        var createdComment = new Comment { Id = 999, Body = commentInput.Body };

        _commentServiceMock
            .Setup(s => s.AddReply(parentCommentId, commentInput))
            .ReturnsAsync(createdComment);

        var result = await _controller.AddReply(parentCommentId, commentInput);
        var createdResult = result as CreatedResult;

        Assert.IsNotNull(createdResult);
        Assert.IsNotNull(createdResult.Value);
        Assert.AreEqual(201, createdResult.StatusCode);
        Assert.AreEqual($"api/comments/{parentCommentId}/replies/{createdComment.Id}", createdResult.Location);
        Assert.AreSame(createdComment, createdResult.Value);

        _commentServiceMock.Verify(s => s.AddReply(parentCommentId, commentInput), Times.Once);
        _commentServiceMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task UpdateCommentReturnsOkWithUpdatedEntityOnSuccess()
    {
        var updatedPost = new Comment { Id = 55, Body = "Updated" };

        _commentServiceMock
            .Setup(s => s.UpdateAsync(updatedPost))
            .Returns(Task.CompletedTask);

        var result = await _controller.UpdateComment(updatedPost);
        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.IsNotNull(okResult.Value);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreSame(updatedPost, okResult.Value);

        _commentServiceMock.Verify(s => s.UpdateAsync(updatedPost), Times.Once);
        _commentServiceMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task DeleteCommentReturnsNoContentOnSuccess()
    {
        long commentId = 88;

        _commentServiceMock
            .Setup(s => s.DeleteById(commentId))
            .Returns(Task.CompletedTask);

        var result = await _controller.DeleteComment(commentId);
        var noContentResult = result as NoContentResult;

        Assert.IsNotNull(noContentResult);
        Assert.AreEqual(204, noContentResult.StatusCode);

        _commentServiceMock.Verify(s => s.DeleteById(commentId), Times.Once);
        _commentServiceMock.VerifyNoOtherCalls();
    }
}