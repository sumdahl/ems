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

        if (dto.EndDate < dto.StartDate)
        {
            return BadRequest("End Date cannot be before Start Date.");
        }

        // 3. Validation: Check if requested days exceed available balance or policy limits
        var requestedDays = (int)(dto.EndDate - dto.StartDate).TotalDays + 1;
            
        switch (leaveType)
        {
            case LeaveType.Annual:
                if (requestedDays > employee.AnnualLeaveBalance)
                {
                    return BadRequest($"Insufficient Annual Leave Balance. You requested {requestedDays} days, but only have {employee.AnnualLeaveBalance} days remaining.");
                }
                break;

            case LeaveType.Sick:
                if (requestedDays > employee.SickLeaveBalance)
                {
                    return BadRequest($"Insufficient Sick Leave Balance. You requested {requestedDays} days, but only have {employee.SickLeaveBalance} days remaining.");
                }
                break;

            case LeaveType.Personal:
                // Policy: Max 3 consecutive days
                if (requestedDays > 3)
                {
                    return BadRequest("Personal leave cannot exceed 3 consecutive days per request.");
                }
                // Balance Check
                if (requestedDays > employee.PersonalLeaveBalance)
                {
                    return BadRequest($"Insufficient Personal Leave Balance. You requested {requestedDays} days, but only have {employee.PersonalLeaveBalance} days remaining.");
                }
                break;

            case LeaveType.Unpaid:
                // Policy: Max 30 days per request
                if (requestedDays > 30)
                {
                     return BadRequest("Unpaid leave cannot exceed 30 days per request.");
                }
                break;

            case LeaveType.Maternity:
                // Policy: Female only
                if (employee.Gender != Gender.Female)
                {
                     return BadRequest("Maternity leave is only applicable for female employees.");
                }
                // Policy: Max 180 days
                if (requestedDays > 180)
                {
                     return BadRequest("Maternity leave cannot exceed 180 days.");
                }
                break;

            case LeaveType.Paternity:
                // Policy: Male only
                if (employee.Gender != Gender.Male)
                {
                     return BadRequest("Paternity leave is only applicable for male employees.");
                }
                // Policy: Max 15 days
                if (requestedDays > 15)
                {
                     return BadRequest("Paternity leave cannot exceed 15 days.");
                }
                break;
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
        
        if (status == LeaveStatus.Approved)
        {
             // Deduct leave balance
             if (request.LeaveType == LeaveType.Annual)
             {
                 request.Employee.AnnualLeaveBalance -= request.TotalDays;
             }
             else if (request.LeaveType == LeaveType.Sick)
             {
                 request.Employee.SickLeaveBalance -= request.TotalDays;
             }
             else if (request.LeaveType == LeaveType.Personal)
             {
                 request.Employee.PersonalLeaveBalance -= request.TotalDays;
             }
        }
        
        // Notify
        await _notificationService.SendSystemUpdateAsync("LeaveRequests");
        // Could notify specific user via personal notification if implemented

        return NoContent();
    }
}
