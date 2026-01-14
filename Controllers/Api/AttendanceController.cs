using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.ViewModels;

namespace EmployeeManagementSystem.Controllers.Api;

[Route("api/Attendance")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class AttendanceApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AttendanceApiController> _logger;

    public AttendanceApiController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<AttendanceApiController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Get attendance records with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<Attendance>>>> GetAttendance(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int? employeeId)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId ?? "");
            var isManager = User.IsInRole("Manager") || User.IsInRole("Admin");

            IQueryable<Attendance> query;

            if (isManager && employeeId.HasValue)
            {
                // Managers can view specific employee's attendance
                query = _context.Attendances
                    .Include(a => a.Employee)
                    .Where(a => a.EmployeeId == employeeId.Value);
            }
            else if (isManager)
            {
                // Managers can view all attendance
                query = _context.Attendances.Include(a => a.Employee);
            }
            else
            {
                // Employees can only view their own attendance
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user!.Email);
                if (employee == null)
                {
                    return NotFound(new ApiResponse<List<Attendance>>
                    {
                        Success = false,
                        Message = "Employee record not found"
                    });
                }

                query = _context.Attendances
                    .Include(a => a.Employee)
                    .Where(a => a.EmployeeId == employee.Id);
            }

            if (startDate.HasValue)
            {
                query = query.Where(a => a.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(a => a.Date <= endDate.Value);
            }

            var attendances = await query.OrderByDescending(a => a.Date).ToListAsync();

            return Ok(new ApiResponse<List<Attendance>>
            {
                Success = true,
                Message = $"Retrieved {attendances.Count} attendance records",
                Data = attendances
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attendance records");
            return StatusCode(500, new ApiResponse<List<Attendance>>
            {
                Success = false,
                Message = "An error occurred while retrieving attendance records"
            });
        }
    }

    /// <summary>
    /// Check in for the current user
    /// </summary>
    [HttpPost("checkin")]
    public async Task<ActionResult<ApiResponse<Attendance>>> CheckIn([FromBody] AttendanceCheckInRequest request)
    {
        try
        {
            if (User.IsInRole("Admin"))
            {
                return StatusCode(403, new ApiResponse<Attendance>
                {
                    Success = false,
                    Message = "Administrators are not required to check in."
                });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId ?? "");
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user!.Email);

            if (employee == null)
            {
                return NotFound(new ApiResponse<Attendance>
                {
                    Success = false,
                    Message = "Employee record not found"
                });
            }

            var today = DateTime.UtcNow.Date;
            var existingAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id && a.Date.Date == today);

            if (existingAttendance != null)
            {
                return BadRequest(new ApiResponse<Attendance>
                {
                    Success = false,
                    Message = "Already checked in today"
                });
            }

            var now = DateTime.UtcNow;
            var attendance = new Attendance
            {
                EmployeeId = employee.Id,
                Date = today,
                CheckInTime = now.TimeOfDay,
                Status = now.TimeOfDay > new TimeSpan(9, 0, 0) ? AttendanceStatus.Late : AttendanceStatus.Present,
                Notes = request.Notes,
                CreatedAt = now
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            await _context.Entry(attendance).Reference(a => a.Employee).LoadAsync();

            _logger.LogInformation("Employee {EmployeeId} checked in at {Time}", employee.Id, now);

            return Ok(new ApiResponse<Attendance>
            {
                Success = true,
                Message = "Checked in successfully",
                Data = attendance
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during check-in");
            return StatusCode(500, new ApiResponse<Attendance>
            {
                Success = false,
                Message = "An error occurred during check-in"
            });
        }
    }

    /// <summary>
    /// Check out for the current user
    /// </summary>
    [HttpPost("checkout/{id}")]
    public async Task<ActionResult<ApiResponse<Attendance>>> CheckOut(int id)
    {
        try
        {
            if (User.IsInRole("Admin"))
            {
                return StatusCode(403, new ApiResponse<Attendance>
                {
                    Success = false,
                    Message = "Administrators are not required to check out."
                });
            }

            var attendance = await _context.Attendances
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (attendance == null)
            {
                return NotFound(new ApiResponse<Attendance>
                {
                    Success = false,
                    Message = "Attendance record not found"
                });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId ?? "");
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user!.Email);

            if (employee == null || attendance.EmployeeId != employee.Id)
            {
                return Forbid();
            }

            if (attendance.CheckOutTime.HasValue)
            {
                return BadRequest(new ApiResponse<Attendance>
                {
                    Success = false,
                    Message = "Already checked out"
                });
            }

            var now = DateTime.UtcNow;
            attendance.CheckOutTime = now.TimeOfDay;
            attendance.UpdatedAt = now;

            if (attendance.CheckInTime.HasValue)
            {
                var hoursWorked = (attendance.CheckOutTime.Value - attendance.CheckInTime.Value).TotalHours;
                attendance.HoursWorked = (decimal)hoursWorked;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Employee {EmployeeId} checked out at {Time}", employee.Id, now);

            return Ok(new ApiResponse<Attendance>
            {
                Success = true,
                Message = "Checked out successfully",
                Data = attendance
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during check-out");
            return StatusCode(500, new ApiResponse<Attendance>
            {
                Success = false,
                Message = "An error occurred during check-out"
            });
        }
    }
}

public class AttendanceCheckInRequest
{
    public string? Notes { get; set; }
}
