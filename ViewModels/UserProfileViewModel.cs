using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.ViewModels;

public class UserProfileViewModel
{
    public ApplicationUser User { get; set; } = default!;
    public Employee? Employee { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
}
