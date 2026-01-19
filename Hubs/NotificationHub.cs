using Microsoft.AspNetCore.SignalR;

namespace EmployeeManagementSystem.Hubs;

public class NotificationHub : Hub
{
    public async Task SendNotification(string message)
    {
        await Clients.All.SendAsync("ReceiveNotification", message);
    }

    public async Task SendSystemUpdate(string updateType)
    {
        await Clients.All.SendAsync("ReceiveSystemUpdate", updateType);
    }
}
