using Forum.Application;
using Forum.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Api.Controllers;

[ApiController]
[Route("api/posts/{postId}/comments")]
public sealed class CommentController(ICommentService commentService) : ControllerBase
{
    private readonly ICommentService _commentService = commentService;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllComments()
    {
        return Ok(await _commentService.GetAllAsync());
    }

    [HttpGet("by-author/{authorId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllCommentsByAuthorId(long authorId)
    {
        return Ok(await _commentService.GetAllByAuthorId(authorId));
    }

    [HttpGet("{commentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCommentById(long commentId)
    {
        return Ok(await _commentService.FindByIdAsync(commentId));
    }

    [HttpPost("{commentId}/replies")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddReply(long commentId, [FromBody] Comment reply)
    {
        var created = await _commentService.AddReply(commentId, reply);
        return Created($"api/comments/{commentId}/replies/{created.Id}", created);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateComment([FromBody] Comment updatedComment)
    {
        await _commentService.UpdateAsync(updatedComment);
        return Ok(updatedComment);
    }

    [HttpDelete("{commentId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteComment(long commentId)
    {
        await _commentService.DeleteById(commentId);
        return NoContent();
    }
}