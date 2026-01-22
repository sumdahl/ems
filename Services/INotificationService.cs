using System.Security.Claims;

namespace EmployeeManagementSystem.Services;

public interface INotificationService
{
    Task<int> GetPendingLeaveRequestsCountAsync(ClaimsPrincipal user);
    Task<int> GetPendingAttendanceCountAsync(ClaimsPrincipal user);
    
    // Targeted notification methods
    Task SendToUserAsync(string userId, string message);
    Task SendToRolesAsync(string[] roles, string message);
    Task SendToAdminsAndManagersAsync(string message);
    Task SendToAdminsOnlyAsync(string message);
    
    // System updates (non-sensitive, for data refresh)
    Task SendSystemUpdateAsync(string updateType);
    Task SendEmployeeUpdateAsync(int employeeId);
    Task SendUserUpdateAsync(string userId);
}
