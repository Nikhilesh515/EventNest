using EventNest.TagService.Application.DTOs.Tags;
using EventNest.TagService.Application.Services.Interfaces;
using EventNest.Shared.Application.Authorization;
using EventNest.Shared.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventNest.TagService.API.Controllers;

[ApiController]
[Route("api/tags")]
public class TagController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpPost]
    [Authorize(EventNestPermissions.Tags.Create)]
    public async Task<IActionResult> Create([FromBody] CreateTagRequestDto request)
    {
        var result = await _tagService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponseDto<TagDto>.Ok(result));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _tagService.GetAllAsync();
        return Ok(ApiResponseDto<List<TagDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _tagService.GetByIdAsync(id);
        if (result is null)
            return NotFound(ApiResponseDto<TagDto>.Fail(404, $"Tag with ID '{id}' was not found."));

        return Ok(ApiResponseDto<TagDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(EventNestPermissions.Tags.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTagRequestDto request)
    {
        var result = await _tagService.UpdateAsync(id, request);
        return Ok(ApiResponseDto<TagDto>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(EventNestPermissions.Tags.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _tagService.DeleteAsync(id);
        return NoContent();
    }
}
