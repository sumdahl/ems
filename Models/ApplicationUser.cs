using Microsoft.AspNetCore.Identity;

namespace EmployeeManagementSystem.Models;

public class ApplicationUser : IdentityUser
{
    public int? EmployeeId { get; set; }
    public string? FullName { get; set; }
    
    // Navigation property
    public virtual Employee? Employee { get; set; }
}
