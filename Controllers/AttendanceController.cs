using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Controllers;

[Authorize]
public class AttendanceController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AttendanceController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Attendance
    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
    {
        var user = await _userManager.GetUserAsync(User);
        var isManager = User.IsInRole("Manager") || User.IsInRole("Admin");

        IQueryable<Attendance> attendances;
        int? currentEmployeeId = null;

        if (isManager)
        {
            // Managers see all attendance records
            attendances = _context.Attendances.Include(a => a.Employee);
        }
        else
        {
            // Employees see only their own records
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user!.Email);
            if (employee == null)
            {
                return NotFound("Employee record not found.");
            }
            attendances = _context.Attendances
                .Include(a => a.Employee)
                .Where(a => a.EmployeeId == employee.Id);
            
            currentEmployeeId = employee.Id;
            // Set current employee ID for check-out button visibility
            ViewBag.CurrentEmployeeId = employee.Id;
        }

        if (startDate.HasValue)
        {
            attendances = attendances.Where(a => a.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            attendances = attendances.Where(a => a.Date <= endDate.Value);
        }

        // Get heatmap data for the last 365 days
        var oneYearAgo = DateTime.UtcNow.AddDays(-365).Date;
        var heatmapQuery = _context.Attendances.AsQueryable();
        
        if (!isManager && currentEmployeeId.HasValue)
        {
            // For employees, show only their own attendance in heatmap
            heatmapQuery = heatmapQuery.Where(a => a.EmployeeId == currentEmployeeId.Value);
        }
        
        var heatmapData = await heatmapQuery
            .Where(a => a.Date >= oneYearAgo && (a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late))
            .GroupBy(a => a.Date.Date)
            .Select(g => new ViewModels.AttendanceHeatmapData
            {
                Date = g.Key,
                Count = g.Count()
            })
            .OrderBy(h => h.Date)
            .ToListAsync();

        ViewBag.StartDate = startDate;
        ViewBag.EndDate = endDate;
        ViewBag.HeatmapData = heatmapData;
        ViewBag.IsManager = isManager;
        ViewBag.IsAdmin = User.IsInRole("Admin");

        return View(await attendances.OrderByDescending(a => a.Date).ToListAsync());
    }

    // GET: Attendance/CheckIn
    public async Task<IActionResult> CheckIn()
    {
        if (User.IsInRole("Admin"))
        {
            TempData["Error"] = "Administrators are not required to check in.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.GetUserAsync(User);
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user!.Email);

        if (employee == null)
        {
            TempData["Error"] = "Employee record not found.";
            return RedirectToAction(nameof(Index));
        }

        // Check if already checked in today
        var today = DateTime.UtcNow.Date;
        var existingAttendance = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id && a.Date.Date == today);

        if (existingAttendance != null)
        {
            TempData["Error"] = "You have already checked in today.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Employee = employee;
        return View();
    }

    // POST: Attendance/CheckIn
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn([Bind("Notes")] Attendance attendance)
    {
        if (User.IsInRole("Admin"))
        {
            TempData["Error"] = "Administrators are not required to check in.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.GetUserAsync(User);
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user!.Email);

        if (employee == null)
        {
            TempData["Error"] = "Employee record not found.";
            return RedirectToAction(nameof(Index));
        }

        var now = DateTime.UtcNow;
        var today = now.Date;

        // Check if already checked in
        var existingAttendance = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id && a.Date.Date == today);

        if (existingAttendance != null)
        {
            TempData["Error"] = "Already checked in today.";
            return RedirectToAction(nameof(Index));
        }

        attendance.EmployeeId = employee.Id;
        attendance.Date = today;
        attendance.CheckInTime = now.TimeOfDay;
        attendance.Status = now.TimeOfDay > new TimeSpan(9, 0, 0) ? AttendanceStatus.Late : AttendanceStatus.Present;
        attendance.CreatedAt = now;

        _context.Add(attendance);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Checked in successfully.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Attendance/CheckOut/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckOut(int id)
    {
        if (User.IsInRole("Admin"))
        {
            TempData["Error"] = "Administrators are not required to check out.";
            return RedirectToAction(nameof(Index));
        }

        var attendance = await _context.Attendances.FindAsync(id);

        if (attendance == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user!.Email);

        if (employee == null || attendance.EmployeeId != employee.Id)
        {
            return Forbid();
        }

        if (attendance.CheckOutTime.HasValue)
        {
            TempData["Error"] = "Already checked out.";
            return RedirectToAction(nameof(Index));
        }

        var now = DateTime.UtcNow;
        attendance.CheckOutTime = now.TimeOfDay;
        attendance.UpdatedAt = now;

        // Calculate hours worked
        if (attendance.CheckInTime.HasValue)
        {
            var hoursWorked = (attendance.CheckOutTime.Value - attendance.CheckInTime.Value).TotalHours;
            attendance.HoursWorked = (decimal)hoursWorked;
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Checked out successfully.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Attendance/Reports
    [Authorize(Policy = "ManagerPolicy")]
    public async Task<IActionResult> Reports(int? departmentId, DateTime? month)
    {
        var selectedMonth = month ?? DateTime.UtcNow;
        var startDate = new DateTime(selectedMonth.Year, selectedMonth.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var query = _context.Attendances
            .Include(a => a.Employee)
            .ThenInclude(e => e.Department)
            .Where(a => a.Date >= startDate && a.Date <= endDate);

        if (departmentId.HasValue)
        {
            query = query.Where(a => a.Employee.DepartmentId == departmentId.Value);
        }

        var attendances = await query
            .OrderBy(a => a.Employee.LastName)
            .ThenBy(a => a.Date)
            .ToListAsync();

        ViewBag.Departments = await _context.Departments.ToListAsync();
        ViewBag.DepartmentId = departmentId;
        ViewBag.Month = selectedMonth;

        return View(attendances);
    }
}
