using EmployeeManagementSystem.Services;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Get pending notification counts
    /// </summary>
    [HttpGet("counts")]
    public async Task<ActionResult<ApiResponse<object>>> GetCounts()
    {
        try
        {
            var pendingLeave = await _notificationService.GetPendingLeaveRequestsCountAsync(User);
            var pendingAttendance = await _notificationService.GetPendingAttendanceCountAsync(User);

            return Ok(new ApiResponse<object>
            {
                 Success = true,
                 Message = "Notification counts retrieved successfully",
                 Data = new
                 {
                     leaveRequests = pendingLeave,
                     attendance = pendingAttendance
                 }
            });
        }
        catch
        {
             return StatusCode(500, new ApiResponse<object> 
             {
                 Success = false,
                 Message = "Error retrieving notification counts"
             });
        }
    }
}
