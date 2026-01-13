using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagementSystem.Models;

public class Employee
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }
    
    [Required]
    [Display(Name = "Hire Date")]
    public DateTime HireDate { get; set; }
    
    [Display(Name = "Termination Date")]
    public DateTime? TerminationDate { get; set; }
    
    [Required]
    [Display(Name = "Department")]
    public int DepartmentId { get; set; }
    
    [Required]
    [Display(Name = "Role")]
    public int RoleId { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Salary { get; set; }
    
    [StringLength(200)]
    public string? Address { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
    
    // Leave balance (in days)
    [Display(Name = "Annual Leave Balance")]
    public int AnnualLeaveBalance { get; set; } = 20;
    [Display(Name = "Sick Leave Balance")]
    public int SickLeaveBalance { get; set; } = 10;
    
    // Navigation properties - not required for validation since we validate the foreign keys
    public virtual Department? Department { get; set; }
    public virtual Role? Role { get; set; }
    public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public virtual ICollection<Attendance> AttendanceRecords { get; set; } = new List<Attendance>();
    public virtual ApplicationUser? User { get; set; }
    
    // Computed property
    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";
}
