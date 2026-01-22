using System.Security.Claims;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace EmployeeManagementSystem.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _hubContext = hubContext;
        _userManager = userManager;
    }

    public async Task SendToUserAsync(string userId, string message)
    {
        // Send notification to a specific user (all their connections)
        await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", message);
    }

    public async Task SendToRolesAsync(string[] roles, string message)
    {
        // Send notification to multiple role groups
        await _hubContext.Clients.Groups(roles).SendAsync("ReceiveNotification", message);
    }

    public async Task SendToAdminsAndManagersAsync(string message)
    {
        // Common pattern: send to both Admin and Manager groups
        await _hubContext.Clients.Groups("Admin", "Manager").SendAsync("ReceiveNotification", message);
    }

    public async Task SendToAdminsOnlyAsync(string message)
    {
        // Send notification only to Admin group (for manager leave requests)
        await _hubContext.Clients.Group("Admin").SendAsync("ReceiveNotification", message);
    }

    public async Task SendSystemUpdateAsync(string updateType)
    {
        // System updates are non-sensitive (just triggers data refresh)
        // Safe to broadcast to all authenticated users
        await _hubContext.Clients.All.SendAsync("ReceiveSystemUpdate", updateType);
    }

    public async Task SendEmployeeUpdateAsync(int employeeId)
    {
        // Notify about employee data changes (for real-time UI updates)
        await _hubContext.Clients.All.SendAsync("ReceiveEmployeeUpdate", employeeId);
    }

    public async Task SendUserUpdateAsync(string userId)
    {
        // Notify about user data changes
        await _hubContext.Clients.All.SendAsync("ReceiveUserUpdate", userId);
    }

    public async Task<int> GetPendingLeaveRequestsCountAsync(ClaimsPrincipal user)
    {
        if (user.IsInRole("Admin"))
        {
            // Admin sees ALL pending leave requests (employees + managers)
            return await _context.LeaveRequests.CountAsync(l => l.Status == LeaveStatus.Pending);
        }
        else if (user.IsInRole("Manager"))
        {
            // Manager sees only employee-submitted pending requests (exclude manager requests)
            // Get all manager emails
            var managerUsers = await _userManager.GetUsersInRoleAsync("Manager");
            var managerEmails = managerUsers.Select(u => u.Email!.ToLower()).ToHashSet();
            
            // Count pending requests where the employee is NOT a manager
            return await _context.LeaveRequests
                .Include(lr => lr.Employee)
                .CountAsync(lr => lr.Status == LeaveStatus.Pending && 
                                  lr.Employee.Email != null && 
                                  !managerEmails.Contains(lr.Employee.Email.ToLower()));
        }
        
        return 0;
    }

    public async Task<int> GetPendingAttendanceCountAsync(ClaimsPrincipal user)
    {
        var today = DateTime.UtcNow.Date;
        
        if (user.IsInRole("Admin"))
        {
            // Admin sees ALL active attendance (employees + managers)
            return await _context.Attendances
                .CountAsync(a => a.Date == today && a.CheckOutTime == null);
        }
        else if (user.IsInRole("Manager"))
        {
            // Manager sees only employee active attendance (exclude managers)
            var managerUsers = await _userManager.GetUsersInRoleAsync("Manager");
            var managerEmails = managerUsers.Select(u => u.Email!.ToLower()).ToHashSet();
            
            return await _context.Attendances
                .Include(a => a.Employee)
                .CountAsync(a => a.Date == today && 
                                 a.CheckOutTime == null &&
                                 a.Employee.Email != null &&
                                 !managerEmails.Contains(a.Employee.Email.ToLower()));
        }
        
        return 0;
    }
}
