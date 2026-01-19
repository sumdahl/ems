using System.Security.Claims;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task SendNotificationAsync(string message)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);
    }

    public async Task SendSystemUpdateAsync(string updateType)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveSystemUpdate", updateType);
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
        // For Managers/Admins: Show count of currently active employees (Checked In but not Checked Out).
        // This gives a real-time view of who is currently working.
        if (user.IsInRole("Admin") || user.IsInRole("Manager"))
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Attendances
                .CountAsync(a => a.Date == today && a.CheckOutTime == null);
        }
        
        return 0;
    }
}
