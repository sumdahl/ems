using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(
        ApplicationDbContext context, 
        UserManager<ApplicationUser> userManager, 
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        // Ensure database is migrated to latest version
        await context.Database.MigrateAsync();
        
        string defaultPassword = configuration["SeedSettings:DefaultPassword"] ?? "Default@123";

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
        var departments = new[]
        {
            new Department { Name = "Human Resources", Description = "HR Department" },
            new Department { Name = "Engineering", Description = "Software Development" },
            new Department { Name = "Sales", Description = "Sales and Marketing" },
            new Department { Name = "Finance", Description = "Finance and Accounting" },
            new Department { Name = "Operations", Description = "Operations Management" },
            new Department { Name = "Social Media Marketing", Description = "Social Media and Content Strategy" },
            new Department { Name = "Business Development", Description = "Strategic Partnerships and Growth" }
        };

        foreach (var dept in departments)
        {
             if (!context.Departments.Any(d => d.Name == dept.Name))
             {
                 context.Departments.Add(dept);
             }
        }
        await context.SaveChangesAsync();
        
        // Helper to safely get Dept ID
        int? GetDeptId(string name) => context.Departments.FirstOrDefault(d => d.Name == name)?.Id;

        // Seed Roles (Job Titles)
        var rolesToSeed = new Dictionary<string, List<string>>
        {
            { "Engineering", new List<string> { "Software Engineer", "Senior Software Engineer", "Engineering Manager" } },
            { "Human Resources", new List<string> { "HR Manager", "HR Specialist" } },
            { "Sales", new List<string> { "Sales Representative", "Sales Manager" } },
            { "Finance", new List<string> { "Accountant", "Finance Manager" } },
            { "Operations", new List<string> { "Operations Manager", "Operations Analyst", "Logistics Coordinator" } },
            { "Social Media Marketing", new List<string> { "Social Media Manager", "Content Creator", "SEO Specialist" } },
            { "Business Development", new List<string> { "Business Development Manager", "Growth Strategist" } }
        };

        foreach (var deptEntry in rolesToSeed)
        {
            var deptId = GetDeptId(deptEntry.Key);
            if (!deptId.HasValue) continue; // Skip if department missing

            foreach (var roleTitle in deptEntry.Value)
            {
                if (!context.JobRoles.Any(r => r.Title == roleTitle && r.DepartmentId == deptId.Value))
                {
                    context.JobRoles.Add(new Role { Title = roleTitle, DepartmentId = deptId.Value });
                }
            }
        }
        await context.SaveChangesAsync();
        
        // Helper to safely create user
        async Task CreateUserAsync(string email, string fullName, string role, Gender gender)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = fullName,
                    Gender = gender
                };
                
                var result = await userManager.CreateAsync(user, defaultPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }

        await CreateUserAsync("admin@ems.com", "System Administrator", "Admin", Gender.Male);
        await CreateUserAsync("manager@ems.com", "Department Manager", "Manager", Gender.Male);
        await CreateUserAsync("employee@ems.com", "Normal Employee", "Employee", Gender.Male);
        await CreateUserAsync("sumiran.dahal@ems.com", "Sumiran Dahal", "Employee", Gender.Male);
        await CreateUserAsync("random.dahal@ems.com", "Random Dahal", "Employee", Gender.Female);
        
        // Seed Sample Employees
        // Helper to get Role ID
        int? GetRoleId(string title) => context.JobRoles.FirstOrDefault(r => r.Title == title)?.Id;

        // Sample data requirements
        var engDeptId = GetDeptId("Engineering");
        var hrDeptId = GetDeptId("Human Resources");
        
        var engManagerRoleId = GetRoleId("Engineering Manager");
        var softwareEngRoleId = GetRoleId("Software Engineer");
        var seniorEngRoleId = GetRoleId("Senior Software Engineer");
        var hrManagerRoleId = GetRoleId("HR Manager");

        if (engDeptId.HasValue && engManagerRoleId.HasValue)
        {
             if (!context.Employees.Any(e => e.Email == "manager@ems.com"))
             {
                 context.Employees.Add(new Employee
                 {
                     FirstName = "Department",
                     LastName = "Manager",
                     Email = "manager@ems.com",
                     Phone = "9842123456",
                     HireDate = DateTime.UtcNow.AddYears(-3),
                     DepartmentId = engDeptId.Value,
                     RoleId = engManagerRoleId.Value,
                     Salary = 95000,
                     Address = "Kathmandu, Tokha",
                     IsActive = true,
                     AnnualLeaveBalance = 25,
                     SickLeaveBalance = 10,
                     Gender = Gender.Male
                 });
             }
        }

        if (engDeptId.HasValue && softwareEngRoleId.HasValue)
        {
             if (!context.Employees.Any(e => e.Email == "employee@ems.com"))
             {
                 context.Employees.Add(new Employee
                 {
                     FirstName = "Normal",
                     LastName = "Employee",
                     Email = "employee@ems.com",
                     Phone = "9842000001", // FIXED: Changed to avoid duplicate
                     HireDate = DateTime.UtcNow.AddMonths(-6),
                     DepartmentId = engDeptId.Value,
                     RoleId = softwareEngRoleId.Value,
                     Salary = 65000,
                     Address = "Kathmandu, Tokha",
                     IsActive = true,
                     AnnualLeaveBalance = 20,
                     SickLeaveBalance = 10,
                     Gender = Gender.Male
                 });
             }
        }

        // Additional employees
        if (engDeptId.HasValue && seniorEngRoleId.HasValue && !context.Employees.Any(e => e.Email == "sumiran.dahal@ems.com"))
        {
            context.Employees.Add(new Employee
            {
                FirstName = "Sumiran",
                LastName = "Dahal",
                Email = "sumiran.dahal@ems.com",
                Phone = "9842110123",
                HireDate = DateTime.UtcNow.AddYears(-2),
                DepartmentId = engDeptId.Value,
                RoleId = seniorEngRoleId.Value,
                Salary = 85000,
                Address = "Kathmandu, Dhapasi",
                IsActive = true,
                AnnualLeaveBalance = 22,
                SickLeaveBalance = 10,
                Gender = Gender.Male
            });
        }

        if (hrDeptId.HasValue && hrManagerRoleId.HasValue && !context.Employees.Any(e => e.Email == "random.dahal@ems.com"))
        {
            context.Employees.Add(new Employee
            {
                FirstName = "Random",
                LastName = "Dahal",
                Email = "random.dahal@ems.com",
                Phone = "9842110124",
                HireDate = DateTime.UtcNow.AddYears(-1),
                DepartmentId = hrDeptId.Value,
                RoleId = hrManagerRoleId.Value,
                Salary = 90000,
                Address = "Jhapa, Damak",
                IsActive = true,
                AnnualLeaveBalance = 23,
                SickLeaveBalance = 10,
                Gender = Gender.Female
            });
        }

        await context.SaveChangesAsync();
        
        // Link users
        async Task LinkEmployeeAsync(string userEmail)
        {
            var emp = context.Employees.FirstOrDefault(e => e.Email == userEmail);
            if (emp != null)
            {
                var user = await userManager.FindByEmailAsync(userEmail);
                if (user != null && user.EmployeeId != emp.Id)
                {
                    user.EmployeeId = emp.Id;
                    await userManager.UpdateAsync(user);
                }
            }
        }

        await LinkEmployeeAsync("manager@ems.com");
        await LinkEmployeeAsync("employee@ems.com");
        await LinkEmployeeAsync("sumiran.dahal@ems.com");
        await LinkEmployeeAsync("random.dahal@ems.com");
    }
}
