using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.ViewModels;
using EmployeeManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class DepartmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly ILogger<DepartmentsController> _logger;

    public DepartmentsController(
        ApplicationDbContext context, 
        INotificationService notificationService,
        ILogger<DepartmentsController> logger)
    {
        _context = context;
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all departments, including their assigned manager and roles.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<DepartmentDto>>>> GetDepartments()
    {
        try
        {
            var departments = await _context.Departments
                .Include(d => d.Manager)
                .Include(d => d.Roles)
                .Select(d => new DepartmentDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    ManagerId = d.ManagerId,
                    ManagerName = d.Manager != null ? $"{d.Manager.FirstName} {d.Manager.LastName}" : null,
                    Roles = d.Roles.Select(r => new RoleDto
                    {
                        Id = r.Id,
                        Title = r.Title,
                        Description = r.Description,
                        DepartmentId = r.DepartmentId,
                        DepartmentName = r.Department != null ? r.Department.Name : null
                    }).ToList()
                })
                .ToListAsync();

            return Ok(new ApiResponse<IEnumerable<DepartmentDto>>
            {
                Success = true,
                Message = "Departments retrieved successfully",
                Data = departments
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving departments");
            return StatusCode(500, new ApiResponse<IEnumerable<DepartmentDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving departments"
            });
        }
    }

    /// <summary>
    /// Retrieves a specific department by ID, including its details, manager, and roles.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> GetDepartment(int id)
    {
        try
        {
            var department = await _context.Departments
                .Include(d => d.Manager)
                .Include(d => d.Roles)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
            {
                return NotFound(new ApiResponse<DepartmentDto>
                {
                    Success = false,
                    Message = "Department not found"
                });
            }

            return Ok(new ApiResponse<DepartmentDto>
            {
                Success = true,
                Message = "Department retrieved successfully",
                Data = new DepartmentDto
                {
                    Id = department.Id,
                    Name = department.Name,
                    Description = department.Description,
                    ManagerId = department.ManagerId,
                    ManagerName = department.Manager != null ? $"{department.Manager.FirstName} {department.Manager.LastName}" : null,
                    Roles = department.Roles.Select(r => new RoleDto
                    {
                        Id = r.Id,
                        Title = r.Title,
                        Description = r.Description,
                        DepartmentId = r.DepartmentId,
                        DepartmentName = r.Department != null ? r.Department.Name : null
                    }).ToList()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving department {Id}", id);
            return StatusCode(500, new ApiResponse<DepartmentDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the department"
            });
        }
    }

    /// <summary>
    /// Creates a new department, optionally with initial roles.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> CreateDepartment(DepartmentDto departmentDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<DepartmentDto>
                {
                    Success = false,
                    Message = "Invalid department data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            var department = new Department
            {
                Name = departmentDto.Name,
                Description = departmentDto.Description,
                ManagerId = departmentDto.ManagerId,
                CreatedAt = DateTime.UtcNow
            };

            // Handle Initial Roles
            if (departmentDto.RoleNames != null && departmentDto.RoleNames.Any())
            {
                foreach (var roleName in departmentDto.RoleNames.Where(r => !string.IsNullOrWhiteSpace(r)))
                {
                    department.Roles.Add(new Role
                    {
                        Title = roleName,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
            await _notificationService.SendSystemUpdateAsync("Departments");
            await _notificationService.SendToAdminsAndManagersAsync($"New department '{department.Name}' has been created via API.");

            departmentDto.Id = department.Id;
            // Populate returned roles
            departmentDto.Roles = department.Roles.Select(r => new RoleDto 
            { 
                Id = r.Id, 
                Title = r.Title, 
                DepartmentId = department.Id 
            }).ToList();
            
            _logger.LogInformation("Department {Id} created successfully", department.Id);

            return CreatedAtAction(nameof(GetDepartment), new { id = department.Id }, new ApiResponse<DepartmentDto>
            {
                Success = true,
                Message = "Department created successfully",
                Data = departmentDto
            });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error creating department");
            return BadRequest(new ApiResponse<DepartmentDto>
            {
                Success = false,
                Message = "Error creating department. Verify unique name.",
                Errors = new List<string> { ex.Message }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating department");
            return StatusCode(500, new ApiResponse<DepartmentDto>
            {
                Success = false,
                Message = "An unexpected error occurred"
            });
        }
    }

    /// <summary>
    /// Updates an existing department. Can also add new roles to the department.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateDepartment(int id, DepartmentDto departmentDto)
    {
        try
        {
            if (id != departmentDto.Id && departmentDto.Id != 0)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Department ID mismatch"
                });
            }

            var department = await _context.Departments
                .Include(d => d.Roles)
                .FirstOrDefaultAsync(d => d.Id == id);
                
            if (department == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Department not found"
                });
            }

            department.Name = departmentDto.Name;
            department.Description = departmentDto.Description;
            department.ManagerId = departmentDto.ManagerId;

            // Add New Roles
            if (departmentDto.RoleNames != null && departmentDto.RoleNames.Any())
            {
                foreach (var roleName in departmentDto.RoleNames.Where(r => !string.IsNullOrWhiteSpace(r)))
                {
                    if (!department.Roles.Any(r => r.Title.Equals(roleName, StringComparison.OrdinalIgnoreCase)))
                    {
                        department.Roles.Add(new Role
                        {
                            Title = roleName,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
            await _notificationService.SendSystemUpdateAsync("Departments");
            await _notificationService.SendToAdminsAndManagersAsync($"Department '{department.Name}' has been updated via API.");
            
            _logger.LogInformation("Department {Id} updated successfully", id);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Department updated successfully"
            });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error updating department {Id}", id);
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Error updating department.",
                Errors = new List<string> { ex.Message }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating department {Id}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while updating the department"
            });
        }
    }
    
    /// <summary>
    /// Deletes a department.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteDepartment(int id)
    {
        try
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Department not found"
                });
            }

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
            await _notificationService.SendSystemUpdateAsync("Departments");
            
            _logger.LogInformation("Department {Id} deleted successfully", id);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Department deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting department {Id}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while deleting the department"
            });
        }
    }
}
