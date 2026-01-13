using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Models;

public class Role
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public int? DepartmentId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Department? Department { get; set; }
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
