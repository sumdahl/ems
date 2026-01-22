using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace EmployeeManagementSystem.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        ApplicationDbContext context, 
        UserManager<ApplicationUser> userManager,
        ILogger<DashboardController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Get dashboard statistics
    /// </summary>
    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<DashboardStats>>> GetStats()
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            
            // Calculate role-aware pending leave requests count
            int pendingLeaveCount;
            if (User.IsInRole("Admin"))
            {
                // Admin sees all pending requests
                pendingLeaveCount = await _context.LeaveRequests.CountAsync(lr => lr.Status == LeaveStatus.Pending);
            }
            else if (User.IsInRole("Manager"))
            {
                // Manager sees only employee-submitted pending requests
                var managerUsers = await _userManager.GetUsersInRoleAsync("Manager");
                var managerEmails = managerUsers.Select(u => u.Email!.ToLower()).ToHashSet();
                
                pendingLeaveCount = await _context.LeaveRequests
                    .Include(lr => lr.Employee)
                    .CountAsync(lr => lr.Status == LeaveStatus.Pending && 
                                      lr.Employee.Email != null && 
                                      !managerEmails.Contains(lr.Employee.Email.ToLower()));
            }
            else
            {
                pendingLeaveCount = 0;
            }
            
            var stats = new DashboardStats
            {
                TotalEmployees = await _context.Employees.CountAsync(e => e.IsActive),
                TotalDepartments = await _context.Departments.CountAsync(),
                PendingLeaveRequests = pendingLeaveCount,
                TodayAttendance = await _context.Attendances.CountAsync(a => a.Date == today && a.CheckOutTime == null) // Active count
            };

            return Ok(new ApiResponse<DashboardStats>
            {
                Success = true,
                Message = "Dashboard stats retrieved successfully",
                Data = stats
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard stats");
            return StatusCode(500, new ApiResponse<DashboardStats>
            {
                Success = false,
                Message = "An error occurred while retrieving dashboard stats"
            });
        }
    }

    /// <summary>
    /// Get department distribution statistics
    /// </summary>
    [HttpGet("department-distribution")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DepartmentStat>>>> GetDepartmentDistribution()
    {
        try
        {
            var stats = await _context.Departments
                .Select(d => new DepartmentStat
                {
                    DepartmentName = d.Name,
                    EmployeeCount = d.Employees.Count(e => e.IsActive)
                })
                .ToListAsync();

            return Ok(new ApiResponse<IEnumerable<DepartmentStat>>
            {
                Success = true,
                Message = "Department distribution retrieved successfully",
                Data = stats
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving department distribution");
            return StatusCode(500, new ApiResponse<IEnumerable<DepartmentStat>>
            {
                Success = false,
                Message = "An error occurred while retrieving department distribution"
            });
        }
    }

    /// <summary>
    /// Get attendance trend for the last 30 days
    /// </summary>
    [HttpGet("attendance-trend")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<IEnumerable<AttendanceTrend>>>> GetAttendanceTrend()
    {
        try
        {
            var last30Days = DateTime.UtcNow.AddDays(-30).Date;
            
            var trend = await _context.Attendances
                .Where(a => a.Date >= last30Days)
                .GroupBy(a => a.Date.Date)
                .Select(g => new AttendanceTrend
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(t => t.Date)
                .ToListAsync();

            return Ok(new ApiResponse<IEnumerable<AttendanceTrend>>
            {
                Success = true,
                Message = "Attendance trend retrieved successfully",
                Data = trend
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attendance trend");
            return StatusCode(500, new ApiResponse<IEnumerable<AttendanceTrend>>
            {
                Success = false,
                Message = "An error occurred while retrieving attendance trend"
            });
        }
    }
}
