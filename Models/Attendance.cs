using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagementSystem.Models;

public enum AttendanceStatus
{
    Present,
    Late,
    Absent,
    OnLeave,
    Holiday
}

public class Attendance
{
    public int Id { get; set; }
    
    [Required]
    public int EmployeeId { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    public TimeSpan? CheckInTime { get; set; }
    
    public TimeSpan? CheckOutTime { get; set; }
    
    [Column(TypeName = "decimal(5,2)")]
    public decimal HoursWorked { get; set; }
    
    [Required]
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Employee Employee { get; set; } = null!;
}
