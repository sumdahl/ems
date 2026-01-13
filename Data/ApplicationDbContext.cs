using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Role> JobRoles { get; set; }
    public DbSet<LeaveRequest> LeaveRequests { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Employee configuration
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            
            entity.HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Role)
                .WithMany(r => r.Employees)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Department configuration
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasOne(d => d.Manager)
                .WithMany()
                .HasForeignKey(d => d.ManagerId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        // Role configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasOne(r => r.Department)
                .WithMany(d => d.Roles)
                .HasForeignKey(r => r.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        // LeaveRequest configuration
        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasOne(lr => lr.Employee)
                .WithMany(e => e.LeaveRequests)
                .HasForeignKey(lr => lr.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(lr => lr.ApprovedBy)
                .WithMany()
                .HasForeignKey(lr => lr.ApprovedById)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        // Attendance configuration
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasOne(a => a.Employee)
                .WithMany(e => e.AttendanceRecords)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(a => new { a.EmployeeId, a.Date }).IsUnique();
        });
        
        // ApplicationUser configuration
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasOne(u => u.Employee)
                .WithOne(e => e.User)
                .HasForeignKey<ApplicationUser>(u => u.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
