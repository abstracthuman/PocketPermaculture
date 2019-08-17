using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace PocketPermaculture.Hubs
{
    public class ChatHub : Hub
    {
        public Task SendMessageToGroup(string userName, string groupName, string message)
        {
            return Clients.Group(groupName).SendAsync("Send", $"{userName}: {message}");
        }

        public async Task AddToGroup(string userName, string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync("Send", $"{userName} has joined the group {groupName}.");
        }

        public async Task RemoveFromGroup(string userName, string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            await Clients.Group(groupName).SendAsync("Send", $"{userName} has left the group {groupName}.");
        }

        public Task SendPrivateMessage(string user, string message)
        {
            return Clients.User(user).SendAsync("ReceiveMessage", message);
        }
    }
}
