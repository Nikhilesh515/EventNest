using System.Security.Claims;
using EventNest.RSVPService.Application.DTOs.Rsvps;
using EventNest.RSVPService.Application.Services.Interfaces;
using EventNest.Shared.Application.Authorization;
using EventNest.Shared.Application.DTOs;
using EventNest.Shared.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventNest.RSVPService.API.Controllers;

[ApiController]
public class RsvpController : ControllerBase
{
    private readonly IRsvpService _rsvpService;

    public RsvpController(IRsvpService rsvpService) => _rsvpService = rsvpService;

    [HttpPost("api/events/{eventId:guid}/rsvps")]
    [Authorize(EventNestPermissions.RSVPs.Create)]
    public async Task<IActionResult> Create(Guid eventId, [FromBody] CreateRsvpRequestDto request)
    {
        var (userId, userName) = GetUserInfo();
        var result = await _rsvpService.CreateAsync(eventId, userId, userName, request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponseDto<RsvpDto>.Ok(result));
    }

    [HttpDelete("api/events/{eventId:guid}/rsvps")]
    [Authorize(EventNestPermissions.RSVPs.Cancel)]
    public async Task<IActionResult> Cancel(Guid eventId)
    {
        var (userId, _) = GetUserInfo();
        var rsvp = await _rsvpService.GetByEventIdAsync(eventId);
        var userRsvp = rsvp.FirstOrDefault(r => r.UserId == userId)
            ?? throw new NotFoundException("RSVP not found for this event.");
        await _rsvpService.CancelAsync(userRsvp.Id, userId);
        return NoContent();
    }

    [HttpGet("api/events/{eventId:guid}/rsvps")]
    [Authorize(EventNestPermissions.RSVPs.Manage)]
    public async Task<IActionResult> GetByEventId(Guid eventId)
    {
        var result = await _rsvpService.GetByEventIdAsync(eventId);
        return Ok(ApiResponseDto<List<RsvpDetailDto>>.Ok(result));
    }

    [HttpGet("api/rsvps/{id:guid}")]
    [Authorize(EventNestPermissions.RSVPs.View)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _rsvpService.GetByIdAsync(id);
        if (result is null)
            return NotFound(ApiResponseDto<object>.Fail(404, "RSVP not found."));
        return Ok(ApiResponseDto<RsvpDto>.Ok(result));
    }

    [HttpPut("api/rsvps/{id:guid}")]
    [Authorize(EventNestPermissions.RSVPs.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRsvpRequestDto request)
    {
        var (userId, _) = GetUserInfo();
        var result = await _rsvpService.UpdateAsync(id, userId, request);
        return Ok(ApiResponseDto<RsvpDto>.Ok(result));
    }

    [HttpGet("api/users/{userId:guid}/rsvps")]
    [Authorize(EventNestPermissions.RSVPs.View)]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        var result = await _rsvpService.GetByUserIdAsync(userId);
        return Ok(ApiResponseDto<List<RsvpDetailDto>>.Ok(result));
    }

    private (Guid UserId, string UserName) GetUserInfo()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException("Invalid user identity.");
        var userName = User.FindFirstValue(ClaimTypes.Name) ?? "Unknown User";
        return (userId, userName);
    }
}
