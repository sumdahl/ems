using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Controllers;

[Authorize(Policy = "ManagerPolicy")]
public class DepartmentsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DepartmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
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
    public async Task<IActionResult> Create([Bind("Name,Description")] Department department)
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
            _context.Add(department);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Department created successfully. You can now add employees and assign a manager.";
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

        var department = await _context.Departments.FindAsync(id);
        if (department == null)
        {
            return NotFound();
        }

        if (User.IsInRole("Admin"))
        {
            var managerUsers = await _userManager.GetUsersInRoleAsync("Manager");
            var managerEmails = managerUsers.Select(u => u.Email).ToHashSet();
            
            // Only show Managers who belong to THIS department
            var managers = await _context.Employees
                .Where(e => managerEmails.Contains(e.Email) && e.DepartmentId == id)
                .OrderBy(e => e.FirstName)
                .ToListAsync();

            ViewData["ManagerId"] = new SelectList(managers, "Id", "FullName", department.ManagerId);
        }
        
        return View(department);
    }

    // POST: Departments/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Department department)
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
                var departmentToUpdate = await _context.Departments.FindAsync(id);
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

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Department updated successfully.";
                    return RedirectToAction(nameof(Index));
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
        
        // Validation Failed
        if (User.IsInRole("Admin"))
        {
            var managerUsers = await _userManager.GetUsersInRoleAsync("Manager");
            var managerEmails = managerUsers.Select(u => u.Email).ToHashSet();
            
            var managers = await _context.Employees
                .Where(e => managerEmails.Contains(e.Email) && e.DepartmentId == id)
                .OrderBy(e => e.FirstName)
                .ToListAsync();

            ViewData["ManagerId"] = new SelectList(managers, "Id", "FullName", department.ManagerId);
        }
        return View(department);
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
            .FirstOrDefaultAsync(m => m.Id == id);

        if (department == null)
        {
            return NotFound();
        }

        return View(department);
    }

    // POST: Departments/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department != null)
        {
            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Department deleted successfully.";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool DepartmentExists(int id)
    {
        return _context.Departments.Any(e => e.Id == id);
    }
}
