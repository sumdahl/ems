using Microsoft.AspNetCore.Identity;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        
        // Seed Identity Roles
        string[] roleNames = { "Admin", "Manager", "Employee" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
        
        // Seed Departments
        if (!context.Departments.Any())
        {
            var departments = new[]
            {
                new Department { Name = "Human Resources", Description = "HR Department" },
                new Department { Name = "Engineering", Description = "Software Development" },
                new Department { Name = "Sales", Description = "Sales and Marketing" },
                new Department { Name = "Finance", Description = "Finance and Accounting" },
                new Department { Name = "Operations", Description = "Operations Management" }
            };
            context.Departments.AddRange(departments);
            await context.SaveChangesAsync();
        }
        
        // Seed Roles (Job Titles)
        if (!context.JobRoles.Any())
        {
            var engineeringDept = context.Departments.First(d => d.Name == "Engineering");
            var hrDept = context.Departments.First(d => d.Name == "Human Resources");
            var salesDept = context.Departments.First(d => d.Name == "Sales");
            var financeDept = context.Departments.First(d => d.Name == "Finance");
            
            var roles = new[]
            {
                new Role { Title = "Software Engineer", DepartmentId = engineeringDept.Id },
                new Role { Title = "Senior Software Engineer", DepartmentId = engineeringDept.Id },
                new Role { Title = "Engineering Manager", DepartmentId = engineeringDept.Id },
                new Role { Title = "HR Manager", DepartmentId = hrDept.Id },
                new Role { Title = "HR Specialist", DepartmentId = hrDept.Id },
                new Role { Title = "Sales Representative", DepartmentId = salesDept.Id },
                new Role { Title = "Sales Manager", DepartmentId = salesDept.Id },
                new Role { Title = "Accountant", DepartmentId = financeDept.Id },
                new Role { Title = "Finance Manager", DepartmentId = financeDept.Id }
            };
            context.JobRoles.AddRange(roles);
            await context.SaveChangesAsync();
        }
        
        // Seed Admin User
        var adminEmail = "admin@ems.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "System Administrator"
            };
            
            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        
        // Seed Sample Employees
        if (!context.Employees.Any())
        {
            var engineeringDept = context.Departments.First(d => d.Name == "Engineering");
            var hrDept = context.Departments.First(d => d.Name == "Human Resources");
            var engineerRole = context.JobRoles.First(r => r.Title == "Software Engineer");
            var hrManagerRole = context.JobRoles.First(r => r.Title == "HR Manager");
            
            var employees = new[]
            {
                new Employee
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@ems.com",
                    Phone = "1234567890",
                    HireDate = DateTime.UtcNow.AddYears(-2),
                    DepartmentId = engineeringDept.Id,
                    RoleId = engineerRole.Id,
                    Salary = 75000,
                    Address = "123 Main St",
                    IsActive = true
                },
                new Employee
                {
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@ems.com",
                    Phone = "0987654321",
                    HireDate = DateTime.UtcNow.AddYears(-1),
                    DepartmentId = hrDept.Id,
                    RoleId = hrManagerRole.Id,
                    Salary = 85000,
                    Address = "456 Oak Ave",
                    IsActive = true
                }
            };
            context.Employees.AddRange(employees);
            await context.SaveChangesAsync();
        }
    }
}
