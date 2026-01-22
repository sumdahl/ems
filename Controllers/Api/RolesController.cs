using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class RolesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RolesController> _logger;

    public RolesController(ApplicationDbContext context, ILogger<RolesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all roles
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<RoleDto>>>> GetRoles()
    {
        try
        {
            var roles = await _context.JobRoles
                .Include(r => r.Department)
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    DepartmentId = r.DepartmentId,
                    DepartmentName = r.Department != null ? r.Department.Name : null
                })
                .ToListAsync();

            return Ok(new ApiResponse<IEnumerable<RoleDto>>
            {
                Success = true,
                Message = "Roles retrieved successfully",
                Data = roles
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles");
            return StatusCode(500, new ApiResponse<IEnumerable<RoleDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving roles"
            });
        }
    }

    /// <summary>
    /// Get specific role by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetRole(int id)
    {
        try
        {
            var role = await _context.JobRoles
                .Include(r => r.Department)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
            {
                return NotFound(new ApiResponse<RoleDto>
                {
                    Success = false,
                    Message = "Role not found"
                });
            }

            return Ok(new ApiResponse<RoleDto>
            {
                Success = true,
                Message = "Role retrieved successfully",
                Data = new RoleDto
                {
                    Id = role.Id,
                    Title = role.Title,
                    Description = role.Description,
                    DepartmentId = role.DepartmentId,
                    DepartmentName = role.Department != null ? role.Department.Name : null
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role {Id}", id);
            return StatusCode(500, new ApiResponse<RoleDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the role"
            });
        }
    }

    /// <summary>
    /// Create new role (Admin/Manager only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> CreateRole(RoleDto roleDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<RoleDto>
                {
                    Success = false,
                    Message = "Invalid role data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            var role = new Role
            {
                Title = roleDto.Title,
                Description = roleDto.Description,
                DepartmentId = roleDto.DepartmentId,
                CreatedAt = DateTime.UtcNow
            };

            _context.JobRoles.Add(role);
            await _context.SaveChangesAsync();

            roleDto.Id = role.Id;
            
            _logger.LogInformation("Role {Id} created successfully", role.Id);

            return CreatedAtAction(nameof(GetRole), new { id = role.Id }, new ApiResponse<RoleDto>
            {
                Success = true,
                Message = "Role created successfully",
                Data = roleDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return StatusCode(500, new ApiResponse<RoleDto>
            {
                Success = false,
                Message = "An error occurred while creating the role"
            });
        }
    }

    /// <summary>
    /// Update existing role (Admin/Manager only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> UpdateRole(int id, RoleDto roleDto)
    {
        try
        {
            if (id != roleDto.Id && roleDto.Id != 0)
            {
                return BadRequest(new ApiResponse<RoleDto>
                {
                    Success = false,
                    Message = "Role ID mismatch"
                });
            }

            var role = await _context.JobRoles.FindAsync(id);
            if (role == null)
            {
                return NotFound(new ApiResponse<RoleDto>
                {
                    Success = false,
                    Message = "Role not found"
                });
            }

            role.Title = roleDto.Title;
            role.Description = roleDto.Description;
            role.DepartmentId = roleDto.DepartmentId;

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Role {Id} updated successfully", id);

            return Ok(new ApiResponse<RoleDto>
            {
                Success = true,
                Message = "Role updated successfully",
                Data = roleDto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role {Id}", id);
            return StatusCode(500, new ApiResponse<RoleDto>
            {
                Success = false,
                Message = "An error occurred while updating the role"
            });
        }
    }

    /// <summary>
    /// Delete role (Admin/Manager only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteRole(int id)
    {
        try
        {
            var role = await _context.JobRoles.FindAsync(id);
            if (role == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Role not found"
                });
            }

            // Check if role is in use
            if (await _context.Employees.AnyAsync(e => e.RoleId == id))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Cannot delete role '{role.Title}' because it is assigned to one or more employees."
                });
            }

            _context.JobRoles.Remove(role);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Role {Id} deleted successfully", id);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Role deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role {Id}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while deleting the role"
            });
        }
    }
}
