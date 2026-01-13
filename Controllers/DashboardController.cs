using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.ViewModels;

namespace EmployeeManagementSystem.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DashboardController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var isManager = User.IsInRole("Manager") || User.IsInRole("Admin");

        var stats = new DashboardStats
        {
            TotalEmployees = await _context.Employees.CountAsync(e => e.IsActive),
            TotalDepartments = await _context.Departments.CountAsync(),
            PendingLeaveRequests = await _context.LeaveRequests.CountAsync(lr => lr.Status == LeaveStatus.Pending),
            TodayAttendance = await _context.Attendances
                .CountAsync(a => a.Date.Date == DateTime.UtcNow.Date && 
                    (a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late))
        };

        // Get recent leave requests
        var recentLeaves = await _context.LeaveRequests
            .Include(lr => lr.Employee)
            .OrderByDescending(lr => lr.CreatedAt)
            .Take(5)
            .ToListAsync();

        // Get department employee counts
        var departmentStats = await _context.Departments
            .Select(d => new DepartmentStat
            {
                DepartmentName = d.Name,
                EmployeeCount = d.Employees.Count(e => e.IsActive)
            })
            .ToListAsync();

        // Get recent attendance for current employee (if not manager)
        List<Attendance>? recentAttendance = null;
        if (!isManager)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user!.Email);
            if (employee != null)
            {
                recentAttendance = await _context.Attendances
                    .Where(a => a.EmployeeId == employee.Id)
                    .OrderByDescending(a => a.Date)
                    .Take(7)
                    .ToListAsync();
            }
        }

        ViewBag.Stats = stats;
        ViewBag.RecentLeaves = recentLeaves;
        ViewBag.DepartmentStats = departmentStats;
        ViewBag.RecentAttendance = recentAttendance;
        ViewBag.IsManager = isManager;

        return View();
    }
}
