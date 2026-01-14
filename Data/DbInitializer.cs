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
                FullName = "Department Manager"
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
                FullName = "Regular Employee"
            };
            
            var result = await userManager.CreateAsync(employeeUser, "Employee@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(employeeUser, "Employee");
            }
        }
        
        // Seed Sample Employees
        if (!context.Employees.Any())
        {
            var engineeringDept = context.Departments.First(d => d.Name == "Engineering");
            var hrDept = context.Departments.First(d => d.Name == "Human Resources");
            var salesDept = context.Departments.First(d => d.Name == "Sales");
            var engineerRole = context.JobRoles.First(r => r.Title == "Software Engineer");
            var seniorEngineerRole = context.JobRoles.First(r => r.Title == "Senior Software Engineer");
            var engineeringManagerRole = context.JobRoles.First(r => r.Title == "Engineering Manager");
            var hrManagerRole = context.JobRoles.First(r => r.Title == "HR Manager");
            
            var employees = new[]
            {
                // Manager user's employee record
                new Employee
                {
                    FirstName = "Department",
                    LastName = "Manager",
                    Email = "manager@ems.com",
                    Phone = "5551234567",
                    HireDate = DateTime.UtcNow.AddYears(-3),
                    DepartmentId = engineeringDept.Id,
                    RoleId = engineeringManagerRole.Id,
                    Salary = 95000,
                    Address = "789 Manager Blvd",
                    IsActive = true,
                    AnnualLeaveBalance = 25,
                    SickLeaveBalance = 15
                },
                // Employee user's employee record
                new Employee
                {
                    FirstName = "Regular",
                    LastName = "Employee",
                    Email = "employee@ems.com",
                    Phone = "5559876543",
                    HireDate = DateTime.UtcNow.AddMonths(-6),
                    DepartmentId = engineeringDept.Id,
                    RoleId = engineerRole.Id,
                    Salary = 65000,
                    Address = "321 Employee St",
                    IsActive = true,
                    AnnualLeaveBalance = 20,
                    SickLeaveBalance = 10
                },
                // Additional sample employees
                new Employee
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@ems.com",
                    Phone = "1234567890",
                    HireDate = DateTime.UtcNow.AddYears(-2),
                    DepartmentId = engineeringDept.Id,
                    RoleId = seniorEngineerRole.Id,
                    Salary = 85000,
                    Address = "123 Main St",
                    IsActive = true,
                    AnnualLeaveBalance = 22,
                    SickLeaveBalance = 12
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
                    Salary = 90000,
                    Address = "456 Oak Ave",
                    IsActive = true,
                    AnnualLeaveBalance = 23,
                    SickLeaveBalance = 13
                }
            };
            context.Employees.AddRange(employees);
            await context.SaveChangesAsync();
            
            // Link users to their employee records
            var managerEmployee = context.Employees.First(e => e.Email == "manager@ems.com");
            var employeeEmployee = context.Employees.First(e => e.Email == "employee@ems.com");
            
            managerUser = await userManager.FindByEmailAsync("manager@ems.com");
            if (managerUser != null)
            {
                managerUser.EmployeeId = managerEmployee.Id;
                await userManager.UpdateAsync(managerUser);
            }
            
            employeeUser = await userManager.FindByEmailAsync("employee@ems.com");
            if (employeeUser != null)
            {
                employeeUser.EmployeeId = employeeEmployee.Id;
                await userManager.UpdateAsync(employeeUser);
            }
        }
    }
}
