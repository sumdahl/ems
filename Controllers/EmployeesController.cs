using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Controllers;

[Authorize]
public class EmployeesController : Controller
{
    private readonly ApplicationDbContext _context;

    public EmployeesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Employees
    public async Task<IActionResult> Index(string searchString, int? departmentId, bool? isActive)
    {
        var employees = _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Role)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            employees = employees.Where(e => 
                e.FirstName.Contains(searchString) || 
                e.LastName.Contains(searchString) || 
                e.Email.Contains(searchString));
        }

        if (departmentId.HasValue)
        {
            employees = employees.Where(e => e.DepartmentId == departmentId.Value);
        }

        if (isActive.HasValue)
        {
            employees = employees.Where(e => e.IsActive == isActive.Value);
        }

        ViewBag.Departments = await _context.Departments.ToListAsync();
        ViewBag.SearchString = searchString;
        ViewBag.DepartmentId = departmentId;
        ViewBag.IsActive = isActive;

        var result = await employees.OrderBy(e => e.LastName).ToListAsync();

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_EmployeeTable", result);
        }

        return View(result);
    }

    // GET: Employees/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var employee = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Role)
            .Include(e => e.LeaveRequests)
            .Include(e => e.AttendanceRecords)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (employee == null)
        {
            return NotFound();
        }

        return View(employee);
    }

    // GET: Employees/Create
    [Authorize(Policy = "ManagerPolicy")]
    public IActionResult Create()
    {
        ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name");
        ViewData["RoleId"] = new SelectList(_context.JobRoles, "Id", "Title");
        return View();
    }

    // POST: Employees/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "ManagerPolicy")]
    public async Task<IActionResult> Create([Bind("FirstName,LastName,Email,Phone,HireDate,DepartmentId,RoleId,Salary,Address,AnnualLeaveBalance,SickLeaveBalance,PersonalLeaveBalance,Gender")] Employee employee)
    {

        
        if (ModelState.IsValid)
        {
            // Check for duplicate email
            if (await _context.Employees.AnyAsync(e => e.Email == employee.Email))
            {
                ModelState.AddModelError("Email", "An employee with this email already exists.");
                ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);
                ViewData["RoleId"] = new SelectList(_context.JobRoles, "Id", "Title", employee.RoleId);
                return View(employee);
            }

            // Convert HireDate to UTC if it's Unspecified (from HTML form)
            if (employee.HireDate.Kind == DateTimeKind.Unspecified)
            {
                employee.HireDate = DateTime.SpecifyKind(employee.HireDate, DateTimeKind.Utc);
            }
            
            employee.IsActive = true;
            employee.CreatedAt = DateTime.UtcNow;
            _context.Add(employee);

            try 
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Employee created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505")
            {
                // Unique constraint violation
                if (pgEx.ConstraintName == "IX_Employees_Email")
                {
                    ModelState.AddModelError("Email", "An employee with this email already exists.");
                }
                else
                {
                    ModelState.AddModelError("", "A duplicate record exists.");
                }
                
                ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);
                ViewData["RoleId"] = new SelectList(_context.JobRoles, "Id", "Title", employee.RoleId);
                return View(employee);
            }
        }
        
        // Log validation errors

        
        ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);
        ViewData["RoleId"] = new SelectList(_context.JobRoles, "Id", "Title", employee.RoleId);
        return View(employee);
    }

    // GET: Employees/Edit/5
    [Authorize(Policy = "ManagerPolicy")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
        {
            return NotFound();
        }
        ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);
        ViewData["RoleId"] = new SelectList(_context.JobRoles, "Id", "Title", employee.RoleId);
        return View(employee);
    }

    // POST: Employees/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "ManagerPolicy")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Email,Phone,HireDate,TerminationDate,DepartmentId,RoleId,Salary,Address,IsActive,AnnualLeaveBalance,SickLeaveBalance,PersonalLeaveBalance,Gender")] Employee employee)
    {
        if (id != employee.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Convert dates to UTC if they're Unspecified (from HTML form)
                if (employee.HireDate.Kind == DateTimeKind.Unspecified)
                {
                    employee.HireDate = DateTime.SpecifyKind(employee.HireDate, DateTimeKind.Utc);
                }
                if (employee.TerminationDate.HasValue && employee.TerminationDate.Value.Kind == DateTimeKind.Unspecified)
                {
                    employee.TerminationDate = DateTime.SpecifyKind(employee.TerminationDate.Value, DateTimeKind.Utc);
                }
                
                employee.UpdatedAt = DateTime.UtcNow;
                _context.Update(employee);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Employee updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(employee.Id))
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
        ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name", employee.DepartmentId);
        ViewData["RoleId"] = new SelectList(_context.JobRoles, "Id", "Title", employee.RoleId);
        return View(employee);
    }

    // GET: Employees/Delete/5
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var employee = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Role)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (employee == null)
        {
            return NotFound();
        }

        return View(employee);
    }

    // POST: Employees/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee != null)
        {
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            TempData["Destructive"] = "Employee deleted successfully.";
        }

        return RedirectToAction(nameof(Index));
    }

    private bool EmployeeExists(int id)
    {
        return _context.Employees.Any(e => e.Id == id);
    }
}
