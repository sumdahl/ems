using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Services;

namespace EmployeeManagementSystem.Controllers;

[Authorize(Policy = "ManagerPolicy")]
public class DepartmentsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly INotificationService _notificationService;

    public DepartmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, INotificationService notificationService)
    {
        _context = context;
        _userManager = userManager;
        _notificationService = notificationService;
    }

    // GET: Departments
    public async Task<IActionResult> Index()
    {
        var departments = await _context.Departments
            .Include(d => d.Manager)
            .Include(d => d.Employees)
            .ToListAsync();
        return View(departments);
    }

    [HttpGet]
    [AllowAnonymous] // Or appropriate policy
    public async Task<IActionResult> GetRolesByDepartment(int departmentId)
    {
        var roles = await _context.JobRoles
            .Where(r => r.DepartmentId == departmentId)
            .Select(r => new { id = r.Id, title = r.Title })
            .ToListAsync();
        return Json(roles);
    }

    // GET: Departments/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var department = await _context.Departments
            .Include(d => d.Manager)
            .Include(d => d.Employees)
            .Include(d => d.Roles)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (department == null)
        {
            return NotFound();
        }

        return View(department);
    }

    // GET: Departments/Create
    public IActionResult Create()
    {
        // Manager cannot be assigned until Department is created and employees are added to it.
        return View();
    }

    // POST: Departments/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Description")] Department department, List<string> RoleNames)
    {
        // Uniqueness Check
        if (await _context.Departments.AnyAsync(d => d.Name.ToLower() == department.Name.ToLower()))
        {
            ModelState.AddModelError("Name", "Department with this name already exists.");
        }

        if (ModelState.IsValid)
        {
            // Manager cannot be assigned during creation to ensure consistency
            department.ManagerId = null;
            department.CreatedAt = DateTime.UtcNow;

            // Handle Roles
            if (RoleNames != null && RoleNames.Any())
            {
                foreach (var roleName in RoleNames.Where(r => !string.IsNullOrWhiteSpace(r)))
                {
                    department.Roles.Add(new Role
                    {
                        Title = roleName,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            _context.Add(department);
            await _context.SaveChangesAsync();
            await _notificationService.SendSystemUpdateAsync("Departments");
            await _notificationService.SendToAdminsAndManagersAsync($"New department '{department.Name}' has been created.");
            TempData["Success"] = "Department created successfully with roles.";
            return RedirectToAction(nameof(Index));
        }
        return View(department);
    }

    // GET: Departments/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var department = await _context.Departments
            .Include(d => d.Roles) // Include Roles
            .FirstOrDefaultAsync(d => d.Id == id); // Use FirstOrDefault to support Include
            
        if (department == null)
        {
            return NotFound();
        }

        if (User.IsInRole("Admin"))
        {
            // Query managers directly from database to avoid Identity type mismatch
            var managers = await _context.Employees
                .Where(e => e.DepartmentId == id && 
                            _context.Users
                                .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                                .Join(_context.Roles, x => x.ur.RoleId, r => r.Id, (x, r) => new { x.u, r })
                                .Where(x => x.r.Name == "Manager" && x.u.Email == e.Email)
                                .Any())
                .OrderBy(e => e.FirstName)
                .ToListAsync();

            ViewData["ManagerId"] = new SelectList(managers, "Id", "FullName", department.ManagerId);
        }
        
        return View(department);
    }

    // POST: Departments/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Department department, List<string> NewRoleNames)
    {
        if (id != department.Id)
        {
            return NotFound();
        }

        // Uniqueness Check
        if (await _context.Departments.AnyAsync(d => d.Name.ToLower() == department.Name.ToLower() && d.Id != id))
        {
            ModelState.AddModelError("Name", "Department with this name already exists.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var departmentToUpdate = await _context.Departments
                    .Include(d => d.Roles)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (departmentToUpdate == null)
                {
                    return NotFound();
                }

                // Custom Check: Manager must belong to this department
                if (User.IsInRole("Admin") && department.ManagerId.HasValue)
                {
                    var proposedManager = await _context.Employees.FindAsync(department.ManagerId.Value);
                    if (proposedManager != null && proposedManager.DepartmentId != id)
                    {
                        ModelState.AddModelError("ManagerId", "The selected Manager does not belong to this department. Please assign the employee to this department first.");
                    }
                }

                if (ModelState.IsValid)
                {
                    departmentToUpdate.Name = department.Name;
                    departmentToUpdate.Description = department.Description;

                    if (User.IsInRole("Admin"))
                    {
                        departmentToUpdate.ManagerId = department.ManagerId;
                    }

                    // Add New Roles
                    if (NewRoleNames != null && NewRoleNames.Any())
                    {
                        foreach (var roleName in NewRoleNames.Where(r => !string.IsNullOrWhiteSpace(r)))
                        {
                            // Check if role already exists in this department to avoid duplicates
                            if (!departmentToUpdate.Roles.Any(r => r.Title.Equals(roleName, StringComparison.OrdinalIgnoreCase)))
                            {
                                departmentToUpdate.Roles.Add(new Role
                                {
                                    Title = roleName,
                                    CreatedAt = DateTime.UtcNow
                                });
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    await _notificationService.SendSystemUpdateAsync("Departments");
                    await _notificationService.SendToAdminsAndManagersAsync($"Department '{departmentToUpdate.Name}' has been updated.");
                    TempData["Success"] = "Department updated successfully.";
                    return RedirectToAction(nameof(Edit), new { id = departmentToUpdate.Id });
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DepartmentExists(department.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        
        // Validation Failed - Reload data including roles
        var departmentWithRoles = await _context.Departments
            .Include(d => d.Roles)
            .FirstOrDefaultAsync(d => d.Id == id);
            
        if (departmentWithRoles != null)
        {
            // Preserve the user's input for Name and Description
            departmentWithRoles.Name = department.Name;
            departmentWithRoles.Description = department.Description;
            departmentWithRoles.ManagerId = department.ManagerId;
        }
        
        if (User.IsInRole("Admin"))
        {
            // Query managers directly from database to avoid Identity type mismatch
            var managers = await _context.Employees
                .Where(e => e.DepartmentId == id && 
                            _context.Users
                                .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                                .Join(_context.Roles, x => x.ur.RoleId, r => r.Id, (x, r) => new { x.u, r })
                                .Where(x => x.r.Name == "Manager" && x.u.Email == e.Email)
                                .Any())
                .OrderBy(e => e.FirstName)
                .ToListAsync();

            ViewData["ManagerId"] = new SelectList(managers, "Id", "FullName", department.ManagerId);
        }
        
        return View(departmentWithRoles ?? department);
    }

    // GET: Departments/Delete/5
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var department = await _context.Departments
            .Include(d => d.Manager)
            .Include(d => d.Employees)
            .Include(d => d.Roles)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (department == null)
        {
            return NotFound();
        }
        
        // Pass dependency counts to the view
        ViewBag.EmployeeCount = department.Employees?.Count ?? 0;
        ViewBag.RoleCount = department.Roles?.Count ?? 0;

        return View(department);
    }

    // POST: Departments/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var department = await _context.Departments
            .Include(d => d.Employees)
            .Include(d => d.Roles)
            .FirstOrDefaultAsync(d => d.Id == id);
            
        if (department == null)
        {
            return NotFound();
        }
        
        // Check for employees assigned to this department
        var employeeCount = department.Employees?.Count ?? 0;
        if (employeeCount > 0)
        {
            TempData["Error"] = $"Cannot delete department '{department.Name}' because it has {employeeCount} employee(s) assigned. Please reassign or remove the employees first.";
            return RedirectToAction(nameof(Index));
        }
        
        // Check for roles assigned to this department
        var roleCount = department.Roles?.Count ?? 0;
        if (roleCount > 0)
        {
            TempData["Error"] = $"Cannot delete department '{department.Name}' because it has {roleCount} role(s). Please delete the roles first from the Edit page.";
            return RedirectToAction(nameof(Index));
        }
        
        // Safe to delete
        try
        {
            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
            await _notificationService.SendSystemUpdateAsync("Departments");
            await _notificationService.SendToAdminsAndManagersAsync($"Department '{department.Name}' has been deleted.");
            TempData["Destructive"] = "Department deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"An error occurred while deleting the department: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Departments/DeleteRole/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminPolicy")]
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
            TempData["Error"] = $"Cannot delete role '{role.Title}' because it is assigned to one or more employees.";
            return RedirectToAction(nameof(Edit), new { id = role.DepartmentId });
        }

        try
        {
            _context.JobRoles.Remove(role);
            await _context.SaveChangesAsync();
            TempData["Destructive"] = "Role deleted successfully.";
        }
        catch (Exception)
        {
            TempData["Error"] = "An error occurred while deleting the role.";
        }

        return RedirectToAction(nameof(Edit), new { id = role.DepartmentId });
    }

    private bool DepartmentExists(int id)
    {
        return _context.Departments.Any(e => e.Id == id);
    }
}
