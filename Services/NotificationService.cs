using System.Security.Claims;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetPendingLeaveRequestsCountAsync(ClaimsPrincipal user)
    {
        // Only Admins and Managers should see pending requests from others
        if (user.IsInRole("Admin") || user.IsInRole("Manager"))
        {
            return await _context.LeaveRequests.CountAsync(l => l.Status == LeaveStatus.Pending);
        }
        
        return 0;
    }

    public async Task<int> GetPendingAttendanceCountAsync(ClaimsPrincipal user)
    {
        // For Managers/Admins: Notify about "Late" arrivals for the current day.
        // This alerts them to review attendance anomalies immediately.
        if (user.IsInRole("Admin") || user.IsInRole("Manager"))
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Attendances
                .CountAsync(a => a.Date == today && a.Status == AttendanceStatus.Late);
        }
        
        return 0;
    }
}
