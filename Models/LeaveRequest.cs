using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace EmployeeManagementSystem.Models;

public enum LeaveType
{
    Annual,
    Sick,
    Personal,
    Unpaid,
    Maternity,
    Paternity
}

public enum LeaveStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled
}

public class LeaveRequest
{
    public int Id { get; set; }
    
    [Required]
    public int EmployeeId { get; set; }
    
    [Required]
    public LeaveType LeaveType { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    [Required]
    [StringLength(500)]
    public string Reason { get; set; } = string.Empty;
    
    [Required]
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    
    public int? ApprovedById { get; set; }
    
    [StringLength(500)]
    public string? ApproverComments { get; set; }
    
    public DateTime? ApprovedAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Computed property
    [NotMapped]
    public int TotalDays
    {
        get
        {
            return (int)(EndDate.Date - StartDate.Date).TotalDays + 1;
        }
    }
    
    // Navigation properties
    [ValidateNever]
    public virtual Employee Employee { get; set; } = null!;
    
    [ValidateNever]
    public virtual Employee? ApprovedBy { get; set; }
}
