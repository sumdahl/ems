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
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public DepartmentsController(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Retrieves all departments, including their assigned manager and roles.
    /// </summary>
    /// <returns>A list of departments.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartments()
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

        return Ok(departments);
    }

    /// <summary>
    /// Retrieves a specific department by ID, including its details, manager, and roles.
    /// </summary>
    /// <param name="id">The ID of the department.</param>
    /// <returns>The department details.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<DepartmentDto>> GetDepartment(int id)
    {
        var department = await _context.Departments
            .Include(d => d.Manager)
            .Include(d => d.Roles)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department == null)
        {
            return NotFound();
        }

        return Ok(new DepartmentDto
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
        });
    }

    /// <summary>
    /// Creates a new department, optionally with initial roles.
    /// </summary>
    /// <param name="departmentDto">The department creation object.</param>
    /// <returns>The created department.</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<DepartmentDto>> CreateDepartment(DepartmentDto departmentDto)
    {
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
        try
        {
             await _context.SaveChangesAsync();
             await _notificationService.SendSystemUpdateAsync("Departments");
             await _notificationService.SendNotificationAsync($"New department '{department.Name}' has been created via API.");
        }
        catch (DbUpdateException ex)
        {
             return BadRequest(new { message = "Error creating department. Verify unique name.", details = ex.Message });
        }
       

        departmentDto.Id = department.Id;
        // Populate returned roles
        departmentDto.Roles = department.Roles.Select(r => new RoleDto 
        { 
            Id = r.Id, 
            Title = r.Title, 
            DepartmentId = department.Id 
        }).ToList();
        
        return CreatedAtAction(nameof(GetDepartment), new { id = department.Id }, departmentDto);
    }

    /// <summary>
    /// Updates an existing department. Can also add new roles to the department.
    /// </summary>
    /// <param name="id">The ID of the department to update.</param>
    /// <param name="departmentDto">The update object.</param>
    /// <returns>No content if successful.</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateDepartment(int id, DepartmentDto departmentDto)
    {
        if (id != departmentDto.Id && departmentDto.Id != 0)
        {
            return BadRequest();
        }

        var department = await _context.Departments
            .Include(d => d.Roles)
            .FirstOrDefaultAsync(d => d.Id == id);
            
        if (department == null)
        {
            return NotFound();
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

        try
        {
            await _context.SaveChangesAsync();
            await _notificationService.SendSystemUpdateAsync("Departments");
            await _notificationService.SendNotificationAsync($"Department '{department.Name}' has been updated via API.");
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!DepartmentExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        catch (DbUpdateException)
        {
             return BadRequest(new { message = "Error updating department." });
        }

        return NoContent();
    }
    
    /// <summary>
    /// Deletes a specific role from the system.
    /// </summary>
    /// <param name="roleId">The ID of the role to delete.</param>
    /// <returns>No content if successful, or BadRequest if the role is currently assigned to employees.</returns>
    [HttpDelete("roles/{roleId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteRole(int roleId)
    {
        var role = await _context.JobRoles.FindAsync(roleId);
        if (role == null)
        {
            return NotFound();
        }

        // Check if role is in use
        if (await _context.Employees.AnyAsync(e => e.RoleId == roleId))
        {
            return BadRequest(new { message = $"Cannot delete role '{role.Title}' because it is assigned to one or more employees." });
        }

        _context.JobRoles.Remove(role);
        await _context.SaveChangesAsync();
        await _notificationService.SendSystemUpdateAsync("Departments");

        return NoContent();
    }

    /// <summary>
    /// Deletes a department.
    /// </summary>
    /// <param name="id">The ID of the department to delete.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null)
        {
            return NotFound();
        }

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();
        await _notificationService.SendSystemUpdateAsync("Departments");

        return NoContent();
    }

    private bool DepartmentExists(int id)
    {
        return _context.Departments.Any(e => e.Id == id);
    }
}
