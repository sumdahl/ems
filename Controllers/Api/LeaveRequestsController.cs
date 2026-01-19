using System.Security.Claims;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Services;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class LeaveRequestsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly INotificationService _notificationService;

    public LeaveRequestsController(
        ApplicationDbContext context, 
        UserManager<ApplicationUser> userManager,
        INotificationService notificationService)
    {
        _context = context;
        _userManager = userManager;
        _notificationService = notificationService;
    }

    // GET: api/LeaveRequests
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetLeaveRequests()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email);
        if (employee == null) return BadRequest("Employee record not found for current user.");

        IQueryable<LeaveRequest> query = _context.LeaveRequests
            .Include(l => l.Employee)
            .Include(l => l.ApprovedBy);

        // If not manager/admin, filter by own requests
        if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            query = query.Where(l => l.EmployeeId == employee.Id);
        }

        var requests = await query
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new LeaveRequestDto
            {
                Id = l.Id,
                EmployeeId = l.EmployeeId,
                EmployeeName = $"{l.Employee.FirstName} {l.Employee.LastName}",
                LeaveType = l.LeaveType.ToString(),
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                Reason = l.Reason,
                Status = l.Status.ToString(),
                CreatedAt = l.CreatedAt,
                ApproverComments = l.ApproverComments,
                ApprovedById = l.ApprovedById,
                ApprovedByName = l.ApprovedBy != null ? $"{l.ApprovedBy.FirstName} {l.ApprovedBy.LastName}" : null,
                ApprovedAt = l.ApprovedAt
            })
            .ToListAsync();

        return Ok(requests);
    }

    // GET: api/LeaveRequests/5
    [HttpGet("{id}")]
    public async Task<ActionResult<LeaveRequestDto>> GetLeaveRequest(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email);
        
        var request = await _context.LeaveRequests
            .Include(l => l.Employee)
            .Include(l => l.ApprovedBy)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (request == null) return NotFound();

        // Check access
        if (!User.IsInRole("Admin") && !User.IsInRole("Manager") && request.EmployeeId != employee?.Id)
        {
            return Forbid();
        }

        return Ok(new LeaveRequestDto
        {
            Id = request.Id,
            EmployeeId = request.EmployeeId,
            EmployeeName = $"{request.Employee.FirstName} {request.Employee.LastName}",
            LeaveType = request.LeaveType.ToString(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Reason = request.Reason,
            Status = request.Status.ToString(),
            CreatedAt = request.CreatedAt,
            ApproverComments = request.ApproverComments,
            ApprovedById = request.ApprovedById,
            ApprovedByName = request.ApprovedBy != null ? $"{request.ApprovedBy.FirstName} {request.ApprovedBy.LastName}" : null,
            ApprovedAt = request.ApprovedAt
        });
    }

    // POST: api/LeaveRequests
    [HttpPost]
    public async Task<ActionResult<LeaveRequestDto>> CreateLeaveRequest(CreateLeaveRequestDto dto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email);
        if (employee == null) return BadRequest("Employee record not found.");

        // Validate LeaveType
        if (!Enum.TryParse<LeaveType>(dto.LeaveType, true, out var leaveType))
        {
            return BadRequest("Invalid Leave Type.");
        }

        // Basic Validation
        if (dto.EndDate < dto.StartDate)
        {
            return BadRequest("End Date cannot be before Start Date.");
        }
        
        // Check for pending requests overlap? (Optional, but good practice)
        var hasPending = await _context.LeaveRequests
            .AnyAsync(l => l.EmployeeId == employee.Id && l.Status == LeaveStatus.Pending);
            
        if (hasPending)
        {
            return BadRequest("You already have a pending leave request.");
        }

        var request = new LeaveRequest
        {
            EmployeeId = employee.Id,
            LeaveType = leaveType,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Reason = dto.Reason,
            Status = LeaveStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.LeaveRequests.Add(request);
        await _context.SaveChangesAsync();
        
        // Notify
        await _notificationService.SendNotificationAsync($"New Leave Request from {employee.FirstName} {employee.LastName}");
        await _notificationService.SendSystemUpdateAsync("LeaveRequests");

        return CreatedAtAction(nameof(GetLeaveRequest), new { id = request.Id }, new LeaveRequestDto
        {
            Id = request.Id,
            EmployeeId = request.EmployeeId,
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            LeaveType = request.LeaveType.ToString(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Reason = request.Reason,
            Status = request.Status.ToString(),
            CreatedAt = request.CreatedAt
        });
    }

    // PUT: api/LeaveRequests/5/status
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateLeaveStatusDto dto)
    {
        var user = await _userManager.GetUserAsync(User);
        var approver = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user!.Email);
        
        var request = await _context.LeaveRequests.FindAsync(id);
        if (request == null) return NotFound();

        if (!Enum.TryParse<LeaveStatus>(dto.Status, true, out var status))
        {
            return BadRequest("Invalid Status.");
        }

        if (status == LeaveStatus.Pending)
        {
            return BadRequest("Cannot revert to Pending.");
        }

        request.Status = status;
        request.ApproverComments = dto.Comments;
        request.ApprovedById = approver?.Id;
        request.ApprovedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        // Notify
        await _notificationService.SendSystemUpdateAsync("LeaveRequests");
        // Could notify specific user via personal notification if implemented

        return NoContent();
    }
}
