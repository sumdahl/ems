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
    public async Task<IActionResult> Index(LeaveStatus? status, string? searchString)
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

        if (!string.IsNullOrEmpty(searchString))
        {
            searchString = searchString.ToLower();
            leaveRequests = leaveRequests.Where(lr => 
                (lr.Employee.FirstName + " " + lr.Employee.LastName).ToLower().Contains(searchString) ||
                lr.Employee.Email.ToLower().Contains(searchString) ||
                lr.Reason.ToLower().Contains(searchString)
            );
        }

        var requests = await leaveRequests.OrderByDescending(lr => lr.CreatedAt).ToListAsync();

        // Calculate permissions for the view
        var canApproveIds = new HashSet<int>();
        if (User.IsInRole("Admin"))
        {
            // Admin can approve everything
            foreach (var req in requests)
            {
                canApproveIds.Add(req.Id);
            }
        }
        else if (User.IsInRole("Manager"))
        {
            // Manager can approve requests from non-Managers
            // Optimization: Fetch all users with Manager role once
            var managerUsers = await _userManager.GetUsersInRoleAsync("Manager");
            var managerEmails = managerUsers.Select(u => u.Email).ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var req in requests)
            {
                // If requester's email is NOT in managerEmails, then current Manager can approve it
                // Also ensures they can't approve their own request (since they are in managerEmails)
                if (req.Employee?.Email != null && !managerEmails.Contains(req.Employee.Email))
                {
                    canApproveIds.Add(req.Id);
                }
            }
        }

        ViewBag.Status = status;
        ViewBag.CanApproveIds = canApproveIds;
        ViewBag.SearchString = searchString;

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_LeaveRequestTable", requests);
        }

        return View(requests);
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

        // Check if user can approve/reject
        bool canApprove = false;
        if (leaveRequest.Status == LeaveStatus.Pending)
        {
            if (User.IsInRole("Admin"))
            {
                canApprove = true;
            }
            else if (User.IsInRole("Manager"))
            {
                // Manager can only approve if requester is NOT a Manager
                var requesterUser = await _userManager.FindByEmailAsync(leaveRequest.Employee.Email);
                var isRequesterManager = requesterUser != null && await _userManager.IsInRoleAsync(requesterUser, "Manager");
                canApprove = !isRequesterManager;
            }
        }
        ViewBag.CanApprove = canApprove;

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
            var today = DateTime.UtcNow.Date;
            
            // Normalize dates to ensure we compare only dates, ignoring times if any
            var startDate = leaveRequest.StartDate.Date;
            var endDate = leaveRequest.EndDate.Date;

            // 1. Validation: Start Date must be >= Today (Present or Future)
            // You can change 'today' to 'today.AddDays(1)' if you want STRICTLY future dates.
            // Requirement says: "start date from present" so allowing today is generally correct.
             if (startDate < today)
            {
                ModelState.AddModelError("StartDate", "Start date cannot be in the past.");
            }

            // 2. Validation: End Date must be >= Start Date (Standard logic)
            // AND Requirement says: "end date must be always the future date" (relative to what? to start date? to today?)
            // Assuming "End Date must be after or equal to Start Date" is the logical requirement.
            // If the user meant "End Date must be strictly in the future relative to TODAY", then `endDate < today` check covers it.
            // But usually, standard Leave logic is: EndDate >= StartDate.
            
            if (endDate < startDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after or the same as the start date.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Employee = employee;
                return View(leaveRequest);
            }

            // Convert dates to UTC for PostgreSQL compatibility
            leaveRequest.StartDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
            leaveRequest.EndDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
            
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

        // Hierarchy Check: Only Admin can approve Manager's request
        var requesterUser = await _userManager.FindByEmailAsync(leaveRequest.Employee.Email);
        if (requesterUser != null && await _userManager.IsInRoleAsync(requesterUser, "Manager"))
        {
            if (!User.IsInRole("Admin"))
            {
                TempData["Error"] = "Only Administrators can approve leave requests for Managers.";
                return RedirectToAction(nameof(Index));
            }
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

        var leaveRequest = await _context.LeaveRequests
            .Include(lr => lr.Employee)
            .FirstOrDefaultAsync(lr => lr.Id == id);

        if (leaveRequest == null)
        {
            return NotFound();
        }

        // Hierarchy Check: Only Admin can reject Manager's request
        var requesterUser = await _userManager.FindByEmailAsync(leaveRequest.Employee.Email);
        if (requesterUser != null && await _userManager.IsInRoleAsync(requesterUser, "Manager"))
        {
            if (!User.IsInRole("Admin"))
            {
                TempData["Error"] = "Only Administrators can reject leave requests for Managers.";
                return RedirectToAction(nameof(Index));
            }
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
