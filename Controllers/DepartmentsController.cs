using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Controllers;

[Authorize(Policy = "ManagerPolicy")]
public class DepartmentsController : Controller
{
    private readonly ApplicationDbContext _context;

    public DepartmentsController(ApplicationDbContext context)
    {
        _context = context;
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
        return View();
    }

    // POST: Departments/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Description")] Department department)
    {
        if (ModelState.IsValid)
        {
            department.CreatedAt = DateTime.UtcNow;
            _context.Add(department);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Department created successfully.";
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
        return View(department);
    }

    // POST: Departments/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ManagerId")] Department department)
    {
        if (id != department.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(department);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Department updated successfully.";
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
            return RedirectToAction(nameof(Index));
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
