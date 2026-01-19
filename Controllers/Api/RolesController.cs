using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public RolesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
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

        return Ok(roles);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoleDto>> GetRole(int id)
    {
        var role = await _context.JobRoles
            .Include(r => r.Department)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
        {
            return NotFound();
        }

        return Ok(new RoleDto
        {
            Id = role.Id,
            Title = role.Title,
            Description = role.Description,
            DepartmentId = role.DepartmentId,
            DepartmentName = role.Department != null ? role.Department.Name : null
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<RoleDto>> CreateRole(RoleDto roleDto)
    {
        var role = new Role
        {
            Title = roleDto.Title,
            Description = roleDto.Description,
            DepartmentId = roleDto.DepartmentId
        };

        _context.JobRoles.Add(role);
        await _context.SaveChangesAsync();

        roleDto.Id = role.Id;
        return CreatedAtAction(nameof(GetRole), new { id = role.Id }, roleDto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateRole(int id, RoleDto roleDto)
    {
        if (id != roleDto.Id && roleDto.Id != 0)
        {
            return BadRequest();
        }

        var role = await _context.JobRoles.FindAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        role.Title = roleDto.Title;
        role.Description = roleDto.Description;
        role.DepartmentId = roleDto.DepartmentId;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!RoleExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        var role = await _context.JobRoles.FindAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        _context.JobRoles.Remove(role);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool RoleExists(int id)
    {
        return _context.JobRoles.Any(e => e.Id == id);
    }
}
