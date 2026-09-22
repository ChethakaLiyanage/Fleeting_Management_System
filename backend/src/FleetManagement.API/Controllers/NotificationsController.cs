using FleetManagement.Application.DTOs.Notifications;
using FleetManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FleetManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService) =>
        _notificationService = notificationService;

    /// <summary>Get all notifications for a user.</summary>
    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetByUser(Guid userId) =>
        Ok(await _notificationService.GetByUserAsync(userId));

    /// <summary>Get unread notification count for a user.</summary>
    [HttpGet("user/{userId:guid}/unread-count")]
    public async Task<IActionResult> GetUnreadCount(Guid userId) =>
        Ok(new { count = await _notificationService.GetUnreadCountAsync(userId) });

    /// <summary>Create a new notification.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateNotificationRequest request) =>
        Ok(await _notificationService.CreateAsync(request));

    /// <summary>Mark a single notification as read.</summary>
    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return NoContent();
    }

    /// <summary>Mark all notifications as read for a user.</summary>
    [HttpPatch("user/{userId:guid}/read-all")]
    public async Task<IActionResult> MarkAllAsRead(Guid userId)
    {
        await _notificationService.MarkAllAsReadAsync(userId);
        return NoContent();
    }

    /// <summary>Delete a notification.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _notificationService.DeleteAsync(id);
        return NoContent();
    }
}
