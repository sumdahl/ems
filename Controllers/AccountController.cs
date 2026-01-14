using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.ViewModels;
using EmployeeManagementSystem.Data;

namespace EmployeeManagementSystem.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AccountController> _logger;
    private readonly ApplicationDbContext _context;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AccountController> logger,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                ModelState.AddModelError(string.Empty, "Account locked out. Please try again later.");
                return View(model);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }
        }

        return View(model);
    }

    [HttpGet]
    [Authorize(Policy = "AdminPolicy")]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Policy = "AdminPolicy")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");
                
                // Assign role
                await _userManager.AddToRoleAsync(user, model.Role);
                
                // If the user is an Employee or Manager, create an Employee record
                if (model.Role == "Employee" || model.Role == "Manager")
                {
                    // Get the first department and role for initial setup
                    var firstDepartment = await _context.Departments.FirstOrDefaultAsync();
                    var firstRole = await _context.JobRoles.FirstOrDefaultAsync();
                    
                    if (firstDepartment != null && firstRole != null)
                    {
                        var employee = new Employee
                        {
                            FirstName = model.FullName.Split(' ').FirstOrDefault() ?? model.FullName,
                            LastName = model.FullName.Split(' ').Skip(1).FirstOrDefault() ?? "",
                            Email = model.Email,
                            HireDate = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc),
                            DepartmentId = firstDepartment.Id,
                            RoleId = firstRole.Id,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            AnnualLeaveBalance = 20,
                            SickLeaveBalance = 10
                        };
                        
                        _context.Employees.Add(employee);
                        await _context.SaveChangesAsync();
                        
                        // Link the user to the employee
                        user.EmployeeId = employee.Id;
                        await _userManager.UpdateAsync(user);
                        
                        _logger.LogInformation($"Employee record created for user {model.Email}");
                    }
                }
                
                TempData["Success"] = "User registered successfully.";
                return RedirectToAction("Index", "Dashboard");
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");
        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return RedirectToAction("Index", "Dashboard");
        }
    }
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        var employee = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Role)
            .FirstOrDefaultAsync(e => e.Email == user.Email);

        var roles = await _userManager.GetRolesAsync(user);

        var model = new UserProfileViewModel
        {
            User = user,
            Employee = employee,
            Roles = roles
        };

        return View(model);
    }
}
