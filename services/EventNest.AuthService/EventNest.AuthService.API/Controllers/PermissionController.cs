using EventNest.AuthService.Application.Authorization;
using EventNest.AuthService.Application.DTOs;
using EventNest.AuthService.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventNest.AuthService.API.Controllers;

[ApiController]
[Route("api/permissions")]
public class PermissionController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var permissions = new List<PermissionDto>();

        foreach (var group in PermissionGroups.All)
        {
            foreach (var permissionName in group.Value)
            {
                var displayName = GetDisplayName(permissionName);
                permissions.Add(new PermissionDto(permissionName, displayName, group.Key, false));
            }
        }

        return Ok(ApiResponse<List<PermissionDto>>.Ok(permissions));
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetUserPermissions(Guid userId)
    {
        var permissions = await _permissionService.GetUserPermissionsAsync(userId);
        return Ok(ApiResponse<IReadOnlyList<PermissionDto>>.Ok(permissions));
    }

    [HttpPost("grant")]
    public async Task<IActionResult> Grant([FromBody] GrantPermissionRequest request)
    {
        var result = await _permissionService.GrantAsync(request.UserId, request.PermissionName, request.ExpiresAt);
        return Ok(ApiResponse<PermissionDto>.Ok(result));
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] RevokePermissionRequest request)
    {
        await _permissionService.RevokeAsync(request.UserId, request.PermissionName);
        return NoContent();
    }

    [HttpGet("check")]
    public async Task<IActionResult> Check([FromQuery] Guid userId, [FromQuery] string permission)
    {
        var result = await _permissionService.CheckAsync(userId, permission);
        return Ok(ApiResponse<bool>.Ok(result));
    }

    private static string GetDisplayName(string permissionName)
    {
        return permissionName switch
        {
            "Events.View" => "View Events",
            "Events.Create" => "Create Events",
            "Events.Edit" => "Edit Events",
            "Events.Delete" => "Delete Events",
            "Tags.View" => "View Tags",
            "Tags.Create" => "Create Tags",
            "Tags.Edit" => "Edit Tags",
            "Tags.Delete" => "Delete Tags",
            "RSVPs.View" => "View RSVPs",
            "RSVPs.Create" => "Create RSVPs",
            "RSVPs.Manage" => "Manage RSVPs",
            "RSVPs.Cancel" => "Cancel RSVPs",
            "Users.View" => "View Users",
            "Users.Manage" => "Manage Users",
            _ => permissionName
        };
    }
}
