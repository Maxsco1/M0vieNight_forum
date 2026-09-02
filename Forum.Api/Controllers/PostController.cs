using Forum.Application;
using Forum.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Forum.Api.Controllers;

[ApiController]
[Route("api/posts")]
public sealed class PostController(IPostService postService) : ControllerBase
{
    private readonly IPostService _postService = postService;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPosts()
    {
        return Ok(await _postService.GetAllAsync());
    }

    [HttpGet("/by-author/{authorId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllPostsByAuthorId(long authorId)
    {
        return Ok(await _postService.GetAllByAuthorId(authorId));
    }

    [HttpGet("{postId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPostById(long postId)
    {
        return Ok(await _postService.FindByIdAsync(postId));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreatePost([FromBody] Post post)
    {
        var created = await _postService.CreateAsync(post);
        return Created($"api/posts/{created.Id}", created);
    }

    [HttpPost("{postId}/comments")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddComment(long postId, [FromBody] Comment comment)
    {
        var created = await _postService.AddComment(postId, comment);
        return Created($"api/posts/{postId}/comments/{created.Id}", created);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdatePost([FromBody] Post updatedPost)
    {
        await _postService.UpdateAsync(updatedPost);
        return Ok(updatedPost);
    }

    [HttpDelete("{postId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeletePost(long postId)
    {
        await _postService.DeleteById(postId);
        return NoContent();
    }
}