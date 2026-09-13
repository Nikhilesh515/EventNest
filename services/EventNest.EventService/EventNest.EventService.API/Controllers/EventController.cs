using EventNest.EventService.Application.DTOs.Events;
using EventNest.EventService.Application.Services.Interfaces;
using EventNest.Shared.Application.Authorization;
using EventNest.Shared.Application.DTOs;
using EventNest.Shared.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventNest.EventService.API.Controllers;

[ApiController]
[Route("api/events")]
public class EventController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly IUserGrpcClient _userGrpcClient;
    private readonly IAuthorizationService _authorizationService;

    public EventController(
        IEventService eventService,
        IUserGrpcClient userGrpcClient,
        IAuthorizationService authorizationService)
    {
        _eventService = eventService;
        _userGrpcClient = userGrpcClient;
        _authorizationService = authorizationService;
    }

    [HttpPost]
    [Authorize(EventNestPermissions.Events.Create)]
    public async Task<IActionResult> Create([FromBody] CreateEventRequestDto request)
    {
        var (organizerId, organizerName) = await GetOrganizerInfoAsync();
        var result = await _eventService.CreateAsync(request, organizerId, organizerName);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponseDto<EventDto>.Ok(result));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] EventListQueryDto query)
    {
        var canManage = await CanManageEventsAsync();
        var result = await _eventService.GetPagedAsync(query, canManage);
        return Ok(ApiResponseDto<PagedResultDto<EventDto>>.Ok(result));
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyEvents()
    {
        var userId = GetUserId();
        var result = await _eventService.GetByOrganizerAsync(userId);
        return Ok(ApiResponseDto<List<EventDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _eventService.GetByIdAsync(id);
        if (result is null)
            return NotFound(ApiResponseDto<EventDto>.Fail(404, $"Event with ID '{id}' was not found."));

        if (!await CanManageEventsAsync() && result.Status != "Published")
            return NotFound(ApiResponseDto<EventDto>.Fail(404, $"Event with ID '{id}' was not found."));

        return Ok(ApiResponseDto<EventDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(EventNestPermissions.Events.Edit)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEventRequestDto request)
    {
        var result = await _eventService.UpdateAsync(id, request, GetUserId());
        return Ok(ApiResponseDto<EventDto>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(EventNestPermissions.Events.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _eventService.DeleteAsync(id, GetUserId());
        return NoContent();
    }

    [HttpPut("{id:guid}/publish")]
    [Authorize(EventNestPermissions.Events.Edit)]
    public async Task<IActionResult> Publish(Guid id)
    {
        var result = await _eventService.PublishAsync(id, GetUserId());
        return Ok(ApiResponseDto<EventDto>.Ok(result));
    }

    [HttpPut("{id:guid}/cancel")]
    [Authorize(EventNestPermissions.Events.Edit)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _eventService.CancelAsync(id, GetUserId());
        return Ok(ApiResponseDto<EventDto>.Ok(result));
    }

    [HttpPut("{id:guid}/complete")]
    [Authorize(EventNestPermissions.Events.Edit)]
    public async Task<IActionResult> Complete(Guid id)
    {
        var result = await _eventService.CompleteAsync(id, GetUserId());
        return Ok(ApiResponseDto<EventDto>.Ok(result));
    }

    private async Task<bool> CanManageEventsAsync()
    {
        var edit = await _authorizationService.AuthorizeAsync(User, EventNestPermissions.Events.Edit);
        if (edit.Succeeded)
            return true;

        var delete = await _authorizationService.AuthorizeAsync(User, EventNestPermissions.Events.Delete);
        return delete.Succeeded;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(claim, out var userId))
            throw new UnauthorizedException("Invalid user identity.");
        return userId;
    }

    private async Task<(Guid Id, string Name)> GetOrganizerInfoAsync()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException("Invalid user identity.");

        var userName = await _userGrpcClient.GetDisplayNameAsync(userId)
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? "Unknown User";

        return (userId, userName);
    }
}
