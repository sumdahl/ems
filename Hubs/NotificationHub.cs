using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EmployeeManagementSystem.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        var httpContext = Context.GetHttpContext();
        
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            // Add user to role-based groups
            if (httpContext.User.IsInRole("Admin"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admin");
            }
            
            if (httpContext.User.IsInRole("Manager"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Manager");
            }
            
            if (httpContext.User.IsInRole("Employee"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Employee");
            }
            
            // Log connection for debugging (optional)
            Console.WriteLine($"User {userId} connected with ConnectionId: {Context.ConnectionId}");
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        Console.WriteLine($"User {userId} disconnected from ConnectionId: {Context.ConnectionId}");
        
        await base.OnDisconnectedAsync(exception);
    }
}
