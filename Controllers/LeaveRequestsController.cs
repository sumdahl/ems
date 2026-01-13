using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Controllers;

[Authorize]
public class LeaveRequestsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public LeaveRequestsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: LeaveRequests
    public async Task<IActionResult> Index(LeaveStatus? status)
    {
        var user = await _userManager.GetUserAsync(User);
        var isManager = User.IsInRole("Manager") || User.IsInRole("Admin");

        // Check if current user has an employee record
        var currentEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user!.Email);
        ViewBag.HasEmployeeRecord = currentEmployee != null;

        IQueryable<LeaveRequest> leaveRequests;

        if (isManager)
        {
            // Managers and Admins see all leave requests
            leaveRequests = _context.LeaveRequests
                .Include(lr => lr.Employee)
                .Include(lr => lr.ApprovedBy);
            
            // Check if manager/admin has pending requests (only if they have employee record)
            if (currentEmployee != null)
            {
                ViewBag.HasPendingRequest = await _context.LeaveRequests
                    .AnyAsync(lr => lr.EmployeeId == currentEmployee.Id && lr.Status == LeaveStatus.Pending);
            }
        }
        else
        {
            // Employees see only their own requests
            if (currentEmployee == null)
            {
                return NotFound("Employee record not found.");
            }
            leaveRequests = _context.LeaveRequests
                .Include(lr => lr.Employee)
                .Include(lr => lr.ApprovedBy)
                .Where(lr => lr.EmployeeId == currentEmployee.Id);
            
            // Check if employee has pending requests
            ViewBag.HasPendingRequest = await _context.LeaveRequests
                .AnyAsync(lr => lr.EmployeeId == currentEmployee.Id && lr.Status == LeaveStatus.Pending);
        }

        if (status.HasValue)
        {
            leaveRequests = leaveRequests.Where(lr => lr.Status == status.Value);
        }

        ViewBag.Status = status;
        return View(await leaveRequests.OrderByDescending(lr => lr.CreatedAt).ToListAsync());
    }

    // GET: LeaveRequests/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var leaveRequest = await _context.LeaveRequests
            .Include(lr => lr.Employee)
            .Include(lr => lr.ApprovedBy)
            .FirstOrDefaultAsync(lr => lr.Id == id);

        if (leaveRequest == null)
        {
            return NotFound();
        }

        // Check if user has permission to view this request
        var user = await _userManager.GetUserAsync(User);
        var isManager = User.IsInRole("Manager") || User.IsInRole("Admin");
        
        if (!isManager)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user!.Email);
            if (employee == null || leaveRequest.EmployeeId != employee.Id)
            {
                return Forbid();
            }
        }

        return View(leaveRequest);
    }

    // GET: LeaveRequests/Create
    public async Task<IActionResult> Create()
    {
        var user = await _userManager.GetUserAsync(User);
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user!.Email);

        if (employee == null)
        {
            TempData["Error"] = "Employee record not found. Please contact administrator.";
            return RedirectToAction(nameof(Index));
        }

        // Check if employee has any pending leave requests
        var hasPendingRequest = await _context.LeaveRequests
            .AnyAsync(lr => lr.EmployeeId == employee.Id && lr.Status == LeaveStatus.Pending);

        if (hasPendingRequest)
        {
            TempData["Error"] = "You already have a pending leave request. Please wait for it to be approved or rejected before submitting a new request.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Employee = employee;
        return View();
    }

    // POST: LeaveRequests/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("LeaveType,StartDate,EndDate,Reason")] LeaveRequest leaveRequest)
    {
        var user = await _userManager.GetUserAsync(User);
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user!.Email);

        if (employee == null)
        {
            TempData["Error"] = "Employee record not found.";
            return RedirectToAction(nameof(Index));
        }

        if (ModelState.IsValid)
        {
            // Convert dates to UTC for PostgreSQL compatibility
            leaveRequest.StartDate = DateTime.SpecifyKind(leaveRequest.StartDate, DateTimeKind.Utc);
            leaveRequest.EndDate = DateTime.SpecifyKind(leaveRequest.EndDate, DateTimeKind.Utc);
            
            leaveRequest.EmployeeId = employee.Id;
            leaveRequest.Status = LeaveStatus.Pending;
            leaveRequest.CreatedAt = DateTime.UtcNow;

            _context.Add(leaveRequest);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Leave request submitted successfully.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Employee = employee;
        return View(leaveRequest);
    }

    // POST: LeaveRequests/Approve/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "ManagerPolicy")]
    public async Task<IActionResult> Approve(int id, string? comments)
    {
        var leaveRequest = await _context.LeaveRequests
            .Include(lr => lr.Employee)
            .FirstOrDefaultAsync(lr => lr.Id == id);

        if (leaveRequest == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        var approver = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user!.Email);

        leaveRequest.Status = LeaveStatus.Approved;
        leaveRequest.ApprovedById = approver?.Id;
        leaveRequest.ApprovedAt = DateTime.UtcNow;
        leaveRequest.ApproverComments = comments;

        // Deduct leave balance
        var employee = leaveRequest.Employee;
        if (leaveRequest.LeaveType == LeaveType.Annual)
        {
            employee.AnnualLeaveBalance -= leaveRequest.TotalDays;
        }
        else if (leaveRequest.LeaveType == LeaveType.Sick)
        {
            employee.SickLeaveBalance -= leaveRequest.TotalDays;
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Leave request approved successfully.";
        return RedirectToAction(nameof(Index));
    }

    // POST: LeaveRequests/Reject/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "ManagerPolicy")]
    public async Task<IActionResult> Reject(int id, string? comments)
    {
        // Validate that comments are provided for rejection
        if (string.IsNullOrWhiteSpace(comments))
        {
            TempData["Error"] = "Please provide a reason for rejecting this leave request.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var leaveRequest = await _context.LeaveRequests.FindAsync(id);

        if (leaveRequest == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        var approver = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user!.Email);

        leaveRequest.Status = LeaveStatus.Rejected;
        leaveRequest.ApprovedById = approver?.Id;
        leaveRequest.ApprovedAt = DateTime.UtcNow;
        leaveRequest.ApproverComments = comments;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Leave request rejected.";
        return RedirectToAction(nameof(Index));
    }
}
