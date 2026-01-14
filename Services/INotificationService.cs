using System.Security.Claims;

namespace EmployeeManagementSystem.Services;

public interface INotificationService
{
    Task<int> GetPendingLeaveRequestsCountAsync(ClaimsPrincipal user);
    Task<int> GetPendingAttendanceCountAsync(ClaimsPrincipal user);
}
