using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<DashboardStats>> GetStats()
    {
        var today = DateTime.UtcNow.Date;
        
        var stats = new DashboardStats
        {
            TotalEmployees = await _context.Employees.CountAsync(e => e.IsActive),
            TotalDepartments = await _context.Departments.CountAsync(),
            PendingLeaveRequests = await _context.LeaveRequests.CountAsync(l => l.Status == LeaveStatus.Pending),
            TodayAttendance = await _context.Attendances.CountAsync(a => a.Date == today && a.CheckOutTime == null) // Active count
        };

        return Ok(stats);
    }

    [HttpGet("department-distribution")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<DepartmentStat>>> GetDepartmentDistribution()
    {
        var stats = await _context.Departments
            .Select(d => new DepartmentStat
            {
                DepartmentName = d.Name,
                EmployeeCount = d.Employees.Count(e => e.IsActive)
            })
            .ToListAsync();

        return Ok(stats);
    }

    [HttpGet("attendance-trend")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<IEnumerable<AttendanceTrend>>> GetAttendanceTrend()
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

        return Ok(trend);
    }
}
