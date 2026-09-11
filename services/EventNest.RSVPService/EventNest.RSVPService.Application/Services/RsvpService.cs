using EventNest.RSVPService.Application.DTOs.Rsvps;
using EventNest.RSVPService.Application.Services.Interfaces;
using EventNest.RSVPService.Domain.Entities;
using EventNest.RSVPService.Domain.Enums;
using EventNest.Shared.Domain.Exceptions;

namespace EventNest.RSVPService.Application.Services;

public class RsvpService : IRsvpService
{
    private readonly IRsvpRepository _repository;
    private readonly IEventGrpcClient _eventGrpcClient;

    public RsvpService(IRsvpRepository repository, IEventGrpcClient eventGrpcClient)
    {
        _repository = repository;
        _eventGrpcClient = eventGrpcClient;
    }

    public async Task<RsvpDto> CreateAsync(Guid eventId, Guid userId, string userName, CreateRsvpRequestDto request)
    {
        var eventInfo = await _eventGrpcClient.GetEventAsync(eventId)
            ?? throw new NotFoundException($"Event with id {eventId} not found.");

        if (eventInfo.Status != "Published")
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["eventId"] = new[] { "RSVP is only available for published events." }
            });

        var existing = await _repository.GetByEventAndUserAsync(eventId, userId);
        if (existing is not null)
        {
            if (existing.Status == RsvpStatus.Cancelled)
            {
                existing.Reactivate(userName, request.GuestCount, request.Notes);
                await ValidateCapacityAsync(eventId, eventInfo.MaxAttendees, request.GuestCount, excludeId: existing.Id);
                await _repository.SaveChangesAsync();
                return MapToDto(existing);
            }
            throw new ConflictException("Already RSVP'd to this event.");
        }

        await ValidateCapacityAsync(eventId, eventInfo.MaxAttendees, request.GuestCount);

        var rsvp = Rsvp.Create(eventId, userId, userName, request.GuestCount, request.Notes);
        await _repository.AddAsync(rsvp);
        await _repository.SaveChangesAsync();
        return MapToDto(rsvp);
    }

    public async Task<RsvpDto?> GetByIdAsync(Guid id)
    {
        var rsvp = await _repository.GetByIdAsync(id);
        return rsvp is null ? null : MapToDto(rsvp);
    }

    public async Task<List<RsvpDetailDto>> GetByEventIdAsync(Guid eventId)
    {
        var rsvps = await _repository.GetByEventIdAsync(eventId);
        return rsvps.Select(r => new RsvpDetailDto(
            r.Id, r.EventId, r.UserId, r.UserName,
            r.Status.ToString(), r.GuestCount, r.Notes,
            r.RespondedAt, r.CreatedAt, null)).ToList();
    }

    public async Task<List<RsvpDetailDto>> GetByUserIdAsync(Guid userId)
    {
        var rsvps = await _repository.GetByUserIdAsync(userId);
        return rsvps.Select(r => new RsvpDetailDto(
            r.Id, r.EventId, r.UserId, r.UserName,
            r.Status.ToString(), r.GuestCount, r.Notes,
            r.RespondedAt, r.CreatedAt, null)).ToList();
    }

    public async Task<RsvpDto> UpdateAsync(Guid id, Guid userId, UpdateRsvpRequestDto request)
    {
        var rsvp = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"RSVP with id {id} not found.");
        if (rsvp.UserId != userId)
            throw new UnauthorizedException("You can only update your own RSVP.");

        if (request.Status is not null)
        {
            var newStatus = Enum.Parse<RsvpStatus>(request.Status, ignoreCase: true);
            switch (newStatus)
            {
                case RsvpStatus.Confirmed: rsvp.Confirm(); break;
                case RsvpStatus.Declined:  rsvp.Decline(); break;
                case RsvpStatus.Maybe:     rsvp.Maybe();   break;
                case RsvpStatus.Cancelled: rsvp.Cancel();  break;
                default:
                    throw new ValidationException(new Dictionary<string, string[]>
                    {
                        ["status"] = new[] { $"Invalid status: {request.Status}." }
                    });
            }
        }

        if (request.GuestCount.HasValue) rsvp.UpdateGuestCount(request.GuestCount.Value);
        if (request.Notes is not null)    rsvp.UpdateNotes(request.Notes);

        await _repository.SaveChangesAsync();
        return MapToDto(rsvp);
    }

    public async Task CancelAsync(Guid id, Guid userId)
    {
        var rsvp = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException($"RSVP with id {id} not found.");
        if (rsvp.UserId != userId)
            throw new UnauthorizedException("You can only cancel your own RSVP.");
        rsvp.Cancel();
        await _repository.SaveChangesAsync();
    }

    public async Task<int> GetConfirmedCountAsync(Guid eventId)
        => await _repository.GetNonCancelledGuestCountAsync(eventId);

    public async Task<int> GetTotalGuestsAsync(Guid eventId)
        => await _repository.GetNonCancelledGuestCountAsync(eventId);

    private async Task ValidateCapacityAsync(Guid eventId, int maxAttendees, int newGuests, Guid? excludeId = null)
    {
        var currentGuests = await _repository.GetNonCancelledGuestCountAsync(eventId);
        if (currentGuests + newGuests > maxAttendees)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["guestCount"] = new[] { $"Event has reached maximum capacity. Remaining: {maxAttendees - currentGuests}." }
            });
    }

    private static RsvpDto MapToDto(Rsvp r) => new(
        r.Id, r.EventId, r.UserId, r.UserName,
        r.Status.ToString(), r.GuestCount, r.Notes,
        r.RespondedAt, r.CreatedAt);
}
