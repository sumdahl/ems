using EmployeeManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet("counts")]
    public async Task<IActionResult> GetCounts()
    {
        var pendingLeave = await _notificationService.GetPendingLeaveRequestsCountAsync(User);
        var pendingAttendance = await _notificationService.GetPendingAttendanceCountAsync(User);

        return Ok(new
        {
            leaveRequests = pendingLeave,
            attendance = pendingAttendance
        });
    }
}
