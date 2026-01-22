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
[Authorize(AuthenticationSchemes = "Bearer")]
public class LeaveRequestsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly INotificationService _notificationService;
    private readonly ILogger<LeaveRequestsController> _logger;

    public LeaveRequestsController(
        ApplicationDbContext context, 
        UserManager<ApplicationUser> userManager,
        INotificationService notificationService,
        ILogger<LeaveRequestsController> logger)
    {
        _context = context;
        _userManager = userManager;
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Get all leave requests (filtered by role)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<LeaveRequestDto>>>> GetLeaveRequests()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new ApiResponse<IEnumerable<LeaveRequestDto>> { Success = false, Message = "User not found" });

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email);
            if (employee == null) return BadRequest(new ApiResponse<IEnumerable<LeaveRequestDto>> { Success = false, Message = "Employee record not found for current user." });

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

            return Ok(new ApiResponse<IEnumerable<LeaveRequestDto>>
            {
                Success = true,
                Message = $"Retrieved {requests.Count} leave requests",
                Data = requests
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leave requests");
            return StatusCode(500, new ApiResponse<IEnumerable<LeaveRequestDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving leave requests"
            });
        }
    }

    /// <summary>
    /// Get specific leave request by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<LeaveRequestDto>>> GetLeaveRequest(int id)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new ApiResponse<LeaveRequestDto> { Success = false, Message = "User not found" });

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email);
            
            var request = await _context.LeaveRequests
                .Include(l => l.Employee)
                .Include(l => l.ApprovedBy)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (request == null) return NotFound(new ApiResponse<LeaveRequestDto> { Success = false, Message = "Leave request not found" });

            // Check access
            if (!User.IsInRole("Admin") && !User.IsInRole("Manager") && request.EmployeeId != employee?.Id)
            {
                return Forbid();
            }

            return Ok(new ApiResponse<LeaveRequestDto>
            {
                Success = true,
                Message = "Leave request retrieved successfully",
                Data = new LeaveRequestDto
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
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leave request {Id}", id);
            return StatusCode(500, new ApiResponse<LeaveRequestDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the leave request"
            });
        }
    }

    /// <summary>
    /// Create new leave request
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<LeaveRequestDto>>> CreateLeaveRequest(CreateLeaveRequestDto dto)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(new ApiResponse<LeaveRequestDto> { Success = false, Message = "User not found" });

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email);
            if (employee == null) return BadRequest(new ApiResponse<LeaveRequestDto> { Success = false, Message = "Employee record not found." });

            // Validate LeaveType
            if (!Enum.TryParse<LeaveType>(dto.LeaveType, true, out var leaveType))
            {
                return BadRequest(new ApiResponse<LeaveRequestDto> { Success = false, Message = "Invalid Leave Type." });
            }

            if (dto.EndDate < dto.StartDate)
            {
                return BadRequest(new ApiResponse<LeaveRequestDto> { Success = false, Message = "End Date cannot be before Start Date." });
            }

            // Validation: Check if requested days exceed available balance or policy limits
            var requestedDays = (int)(dto.EndDate - dto.StartDate).TotalDays + 1;
                
            switch (leaveType)
            {
                case LeaveType.Annual:
                    if (requestedDays > employee.AnnualLeaveBalance)
                    {
                        return BadRequest(new ApiResponse<LeaveRequestDto> { Success = false, Message = $"Insufficient Annual Leave Balance. You requested {requestedDays} days, but only have {employee.AnnualLeaveBalance} days remaining." });
                        
                    }
                    break;

                case LeaveType.Sick:
                    if (requestedDays > employee.SickLeaveBalance)
                    {
                        return BadRequest(new ApiResponse<LeaveRequestDto> { Success = false, Message = $"Insufficient Sick Leave Balance. You requested {requestedDays} days, but only have {employee.SickLeaveBalance} days remaining." });
                    }
                    break;

                case LeaveType.Personal:
                    // Policy: Max 3 consecutive days
                    if (requestedDays > 3)
                    {
                        return BadRequest(new ApiResponse<LeaveRequestDto> { Success = false, Message = "Personal leave cannot exceed 3 consecutive days per request." });
                    }
                    // Balance Check
                    if (requestedDays > employee.PersonalLeaveBalance)
                    {
                        return BadRequest(new ApiResponse<LeaveRequestDto> { Success = false, Message = $"Insufficient Personal Leave Balance. You requested {requestedDays} days, but only have {employee.PersonalLeaveBalance} days remaining." });
                    }
                    break;

                case LeaveType.Unpaid:
                    // Policy: Max 30 days per request
                    if (requestedDays > 30)
                    {
                         return BadRequest(new ApiResponse<LeaveRequestDto> { Success = false, Message = "Unpaid leave cannot exceed 30 days per request." });
                    }
                    break;

                case LeaveType.Maternity:
                    // Policy: Female only
                    if (employee.Gender != Gender.Female)
                    {
                         return BadRequest(new ApiResponse<LeaveRequestDto> { Success = false, Message = "Maternity leave is only applicable for female employees." });
                    }
                    // Policy: Max 180 days
                    if (requestedDays > 180)
                    {
                         return BadRequest(new ApiResponse<LeaveRequestDto> { Success = false, Message = "Maternity leave cannot exceed 180 days." });
                    }
                    break;

                case LeaveType.Paternity:
                    // Policy: Male only
                    if (employee.Gender != Gender.Male)
                    {
                         return BadRequest(new ApiResponse<LeaveRequestDto> { Success = false, Message = "Paternity leave is only applicable for male employees." });
                    }
                    // Policy: Max 15 days
                    if (requestedDays > 15)
                    {
                         return BadRequest(new ApiResponse<LeaveRequestDto> { Success = false, Message = "Paternity leave cannot exceed 15 days." });
                    }
                    break;
            }
            
            // Check for pending requests overlap
            var hasPending = await _context.LeaveRequests
                .AnyAsync(l => l.EmployeeId == employee.Id && l.Status == LeaveStatus.Pending);
                
            if (hasPending)
            {
                return BadRequest(new ApiResponse<LeaveRequestDto> { Success = false, Message = "You already have a pending leave request." });
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
            
            // Notify only admins/managers (privacy-aware)
            await _notificationService.SendToAdminsAndManagersAsync($"New Leave Request from {employee.FirstName} {employee.LastName}");
            await _notificationService.SendSystemUpdateAsync("LeaveRequests");

            return CreatedAtAction(nameof(GetLeaveRequest), new { id = request.Id }, new ApiResponse<LeaveRequestDto>
            {
                Success = true,
                Message = "Leave request created successfully",
                Data = new LeaveRequestDto
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
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating leave request");
            return StatusCode(500, new ApiResponse<LeaveRequestDto>
            {
                Success = false,
                Message = "An error occurred while creating the leave request"
            });
        }
    }

    /// <summary>
    /// Update leave request status (Approve/Reject)
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateStatus(int id, UpdateLeaveStatusDto dto)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            var approver = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user!.Email);
            
            var request = await _context.LeaveRequests
                .Include(l => l.Employee) // Needed for balance update
                .FirstOrDefaultAsync(l => l.Id == id);

            if (request == null) return NotFound(new ApiResponse<object> { Success = false, Message = "Leave request not found" });

            if (!Enum.TryParse<LeaveStatus>(dto.Status, true, out var status))
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Invalid Status." });
            }

            if (status == LeaveStatus.Pending)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Cannot revert to Pending." });
            }

            request.Status = status;
            request.ApproverComments = dto.Comments;
            request.ApprovedById = approver?.Id;
            request.ApprovedAt = DateTime.UtcNow;

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
            
            await _context.SaveChangesAsync();

            // Notify
            await _notificationService.SendSystemUpdateAsync("LeaveRequests");
            
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = $"Leave request {status.ToString().ToLower()} successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating leave request status {Id}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while updating the leave request"
            });
        }
    }

    /// <summary>
    /// Delete leave request (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteLeaveRequest(int id)
    {
        try
        {
            var request = await _context.LeaveRequests.FindAsync(id);
            if (request == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Leave request not found"
                });
            }

            _context.LeaveRequests.Remove(request);
            await _context.SaveChangesAsync();
            await _notificationService.SendSystemUpdateAsync("LeaveRequests");

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Leave request deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting leave request {Id}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while deleting the leave request"
            });
        }
    }
}
