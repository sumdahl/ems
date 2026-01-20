using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Ensure database is migrated to latest version
        await context.Database.MigrateAsync();
        
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
        
        // Seed Roles (Job Titles)
        // Helper to get Dept ID
        int GetDeptId(string name) => context.Departments.First(d => d.Name == name).Id;

        var roles = new List<Role>
        {
            new Role { Title = "Software Engineer", DepartmentId = GetDeptId("Engineering") },
            new Role { Title = "Senior Software Engineer", DepartmentId = GetDeptId("Engineering") },
            new Role { Title = "Engineering Manager", DepartmentId = GetDeptId("Engineering") },
            new Role { Title = "HR Manager", DepartmentId = GetDeptId("Human Resources") },
            new Role { Title = "HR Specialist", DepartmentId = GetDeptId("Human Resources") },
            new Role { Title = "Sales Representative", DepartmentId = GetDeptId("Sales") },
            new Role { Title = "Sales Manager", DepartmentId = GetDeptId("Sales") },
            new Role { Title = "Accountant", DepartmentId = GetDeptId("Finance") },
            new Role { Title = "Finance Manager", DepartmentId = GetDeptId("Finance") },
            // New Roles for Operations
            new Role { Title = "Operations Manager", DepartmentId = GetDeptId("Operations") },
            new Role { Title = "Operations Analyst", DepartmentId = GetDeptId("Operations") },
            new Role { Title = "Logistics Coordinator", DepartmentId = GetDeptId("Operations") },
            // New Roles for Social Media Marketing
            new Role { Title = "Social Media Manager", DepartmentId = GetDeptId("Social Media Marketing") },
            new Role { Title = "Content Creator", DepartmentId = GetDeptId("Social Media Marketing") },
            new Role { Title = "SEO Specialist", DepartmentId = GetDeptId("Social Media Marketing") },
            // New Roles for Business Development
            new Role { Title = "Business Development Manager", DepartmentId = GetDeptId("Business Development") },
            new Role { Title = "Growth Strategist", DepartmentId = GetDeptId("Business Development") }
        };

        foreach (var role in roles)
        {
            if (!context.JobRoles.Any(r => r.Title == role.Title && r.DepartmentId == role.DepartmentId))
            {
                context.JobRoles.Add(role);
            }
        }
        await context.SaveChangesAsync();
        
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
                FullName = "System Administrator",
                Gender = Gender.Male
            };
            
            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        
        // Seed Manager User
        var managerEmail = "manager@ems.com";
        var managerUser = await userManager.FindByEmailAsync(managerEmail);
        
        if (managerUser == null)
        {
            managerUser = new ApplicationUser
            {
                UserName = managerEmail,
                Email = managerEmail,
                EmailConfirmed = true,
                FullName = "Department Manager",
                Gender = Gender.Male
            };
            
            var result = await userManager.CreateAsync(managerUser, "Manager@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(managerUser, "Manager");
            }
        }
        
        // Seed Employee User
        var employeeEmail = "employee@ems.com";
        var employeeUser = await userManager.FindByEmailAsync(employeeEmail);
        
        if (employeeUser == null)
        {
            employeeUser = new ApplicationUser
            {
                UserName = employeeEmail,
                Email = employeeEmail,
                EmailConfirmed = true,
                FullName = "Regular Employee",
                Gender = Gender.Male
            };
            
            var result = await userManager.CreateAsync(employeeUser, "Employee@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(employeeUser, "Employee");
            }
        }
        
        // Seed Sample Employees
        // Seed Sample Employees
        var engDept = context.Departments.First(d => d.Name == "Engineering");
        var humanResDept = context.Departments.First(d => d.Name == "Human Resources");
        var salesDepartment = context.Departments.First(d => d.Name == "Sales");
        
        var engineerRole = context.JobRoles.First(r => r.Title == "Software Engineer");
        var seniorEngineerRole = context.JobRoles.First(r => r.Title == "Senior Software Engineer");
        var engineeringManagerRole = context.JobRoles.First(r => r.Title == "Engineering Manager");
        var hrManagerRole = context.JobRoles.First(r => r.Title == "HR Manager");

        // Ensure Manager Employee exists
        if (!context.Employees.Any(e => e.Email == "manager@ems.com"))
        {
            var managerEmployee = new Employee
            {
                FirstName = "Department",
                LastName = "Manager",
                Email = "manager@ems.com",
                Phone = "5551234567",
                HireDate = DateTime.UtcNow.AddYears(-3),
                DepartmentId = engDept.Id,
                RoleId = engineeringManagerRole.Id,
                Salary = 95000,
                Address = "789 Manager Blvd",
                IsActive = true,
                AnnualLeaveBalance = 25,
                SickLeaveBalance = 15,
                Gender = Gender.Male
            };
            context.Employees.Add(managerEmployee);
        }

        // Ensure Regular Employee exists
        if (!context.Employees.Any(e => e.Email == "employee@ems.com"))
        {
            var regularEmployee = new Employee
            {
                FirstName = "Regular",
                LastName = "Employee",
                Email = "employee@ems.com",
                Phone = "5559876543",
                HireDate = DateTime.UtcNow.AddMonths(-6),
                DepartmentId = engDept.Id,
                RoleId = engineerRole.Id,
                Salary = 65000,
                Address = "321 Employee St",
                IsActive = true,
                AnnualLeaveBalance = 20,
                SickLeaveBalance = 10,
                Gender = Gender.Male
            };
            context.Employees.Add(regularEmployee);
        }

        // Add other sample employees if none exist (legacy check)
        if (!context.Employees.Any(e => e.Email != "manager@ems.com" && e.Email != "employee@ems.com"))
        {
             var employees = new[]
            {
                new Employee
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@ems.com",
                    Phone = "1234567890",
                    HireDate = DateTime.UtcNow.AddYears(-2),
                    DepartmentId = engDept.Id,
                    RoleId = seniorEngineerRole.Id,
                    Salary = 85000,
                    Address = "123 Main St",
                    IsActive = true,
                    AnnualLeaveBalance = 22,
                    SickLeaveBalance = 12,
                    Gender = Gender.Male
                },
                new Employee
                {
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@ems.com",
                    Phone = "0987654321",
                    HireDate = DateTime.UtcNow.AddYears(-1),
                    DepartmentId = humanResDept.Id,
                    RoleId = hrManagerRole.Id,
                    Salary = 90000,
                    Address = "456 Oak Ave",
                    IsActive = true,
                    AnnualLeaveBalance = 23,
                    SickLeaveBalance = 13,
                    Gender = Gender.Female
                }
            };
            context.Employees.AddRange(employees);
        }
        
        await context.SaveChangesAsync();
        
        // Link users to their employee records
        var managerEmp = context.Employees.FirstOrDefault(e => e.Email == "manager@ems.com");
        var regularEmp = context.Employees.FirstOrDefault(e => e.Email == "employee@ems.com");
        
        if (managerEmp != null)
        {
            managerUser = await userManager.FindByEmailAsync("manager@ems.com");
            if (managerUser != null && managerUser.EmployeeId != managerEmp.Id)
            {
                managerUser.EmployeeId = managerEmp.Id;
                await userManager.UpdateAsync(managerUser);
            }
        }
        
        if (regularEmp != null)
        {
            employeeUser = await userManager.FindByEmailAsync("employee@ems.com");
            if (employeeUser != null && employeeUser.EmployeeId != regularEmp.Id)
            {
                employeeUser.EmployeeId = regularEmp.Id;
                await userManager.UpdateAsync(employeeUser);
            }
        }
    }
}
