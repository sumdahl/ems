using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.ViewModels;
using EmployeeManagementSystem.Services;

namespace EmployeeManagementSystem.Controllers.Api;

[Route("api/Employees")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class EmployeesApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EmployeesApiController> _logger;
    private readonly INotificationService _notificationService;

    public EmployeesApiController(ApplicationDbContext context, ILogger<EmployeesApiController> logger, INotificationService notificationService)
    {
        _context = context;
        _logger = logger;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Get all employees with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<Employee>>>> GetEmployees(
        [FromQuery] string? searchString,
        [FromQuery] int? departmentId,
        [FromQuery] bool? isActive)
    {
        try
        {
            var query = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Role)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(e =>
                    e.FirstName.Contains(searchString) ||
                    e.LastName.Contains(searchString) ||
                    e.Email.Contains(searchString));
            }

            if (departmentId.HasValue)
            {
                query = query.Where(e => e.DepartmentId == departmentId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(e => e.IsActive == isActive.Value);
            }

            var employees = await query.OrderBy(e => e.LastName).ToListAsync();

            return Ok(new ApiResponse<List<Employee>>
            {
                Success = true,
                Message = $"Retrieved {employees.Count} employees",
                Data = employees
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employees");
            return StatusCode(500, new ApiResponse<List<Employee>>
            {
                Success = false,
                Message = "An error occurred while retrieving employees"
            });
        }
    }

    /// <summary>
    /// Get employee by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Employee>>> GetEmployee(int id)
    {
        try
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Role)
                .Include(e => e.LeaveRequests)
                .Include(e => e.AttendanceRecords)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return NotFound(new ApiResponse<Employee>
                {
                    Success = false,
                    Message = "Employee not found"
                });
            }

            return Ok(new ApiResponse<Employee>
            {
                Success = true,
                Message = "Employee retrieved successfully",
                Data = employee
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employee {Id}", id);
            return StatusCode(500, new ApiResponse<Employee>
            {
                Success = false,
                Message = "An error occurred while retrieving the employee"
            });
        }
    }

    /// <summary>
    /// Create new employee (Manager/Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<Employee>>> CreateEmployee([FromBody] Employee employee)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<Employee>
                {
                    Success = false,
                    Message = "Invalid employee data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            employee.CreatedAt = DateTime.UtcNow;
            employee.HireDate = DateTime.SpecifyKind(employee.HireDate, DateTimeKind.Utc);

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            await _notificationService.SendEmployeeUpdateAsync(employee.Id);
            await _notificationService.SendNotificationAsync($"Employee {employee.FirstName} {employee.LastName} has been created via API.");

            // Reload with related data
            await _context.Entry(employee).Reference(e => e.Department).LoadAsync();
            await _context.Entry(employee).Reference(e => e.Role).LoadAsync();

            _logger.LogInformation("Employee {Id} created successfully", employee.Id);

            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, new ApiResponse<Employee>
            {
                Success = true,
                Message = "Employee created successfully",
                Data = employee
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee");
            return StatusCode(500, new ApiResponse<Employee>
            {
                Success = false,
                Message = "An error occurred while creating the employee"
            });
        }
    }

    /// <summary>
    /// Update employee (Manager/Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<Employee>>> UpdateEmployee(int id, [FromBody] Employee employee)
    {
        try
        {
            if (id != employee.Id)
            {
                return BadRequest(new ApiResponse<Employee>
                {
                    Success = false,
                    Message = "Employee ID mismatch"
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<Employee>
                {
                    Success = false,
                    Message = "Invalid employee data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            var existingEmployee = await _context.Employees.FindAsync(id);
            if (existingEmployee == null)
            {
                return NotFound(new ApiResponse<Employee>
                {
                    Success = false,
                    Message = "Employee not found"
                });
            }

            // Update fields
            existingEmployee.FirstName = employee.FirstName;
            existingEmployee.LastName = employee.LastName;
            existingEmployee.Email = employee.Email;
            existingEmployee.Phone = employee.Phone;
            existingEmployee.HireDate = DateTime.SpecifyKind(employee.HireDate, DateTimeKind.Utc);
            existingEmployee.TerminationDate = employee.TerminationDate.HasValue 
                ? DateTime.SpecifyKind(employee.TerminationDate.Value, DateTimeKind.Utc) 
                : null;
            existingEmployee.DepartmentId = employee.DepartmentId;
            existingEmployee.RoleId = employee.RoleId;
            existingEmployee.Salary = employee.Salary;
            existingEmployee.Address = employee.Address;
            existingEmployee.IsActive = employee.IsActive;
            existingEmployee.AnnualLeaveBalance = employee.AnnualLeaveBalance;
            existingEmployee.SickLeaveBalance = employee.SickLeaveBalance;
            existingEmployee.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _notificationService.SendEmployeeUpdateAsync(id);
            await _notificationService.SendNotificationAsync($"Employee {existingEmployee.FirstName} {existingEmployee.LastName} has been updated via API.");

            // Reload with related data
            await _context.Entry(existingEmployee).Reference(e => e.Department).LoadAsync();
            await _context.Entry(existingEmployee).Reference(e => e.Role).LoadAsync();

            _logger.LogInformation("Employee {Id} updated successfully", id);

            return Ok(new ApiResponse<Employee>
            {
                Success = true,
                Message = "Employee updated successfully",
                Data = existingEmployee
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee {Id}", id);
            return StatusCode(500, new ApiResponse<Employee>
            {
                Success = false,
                Message = "An error occurred while updating the employee"
            });
        }
    }

    /// <summary>
    /// Delete employee (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteEmployee(int id)
    {
        try
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Employee not found"
                });
            }

            // Soft delete
            employee.IsActive = false;
            employee.TerminationDate = DateTime.UtcNow;
            employee.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Employee {Id} deleted (soft delete)", id);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Employee deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee {Id}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while deleting the employee"
            });
        }
    }
}
