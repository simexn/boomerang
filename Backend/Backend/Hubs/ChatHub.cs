using Microsoft.AspNetCore.SignalR;
using Backend.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Hubs
{
    [Route("chatHub")]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }

        public async Task JoinRoom(string roomName, string userName, int userId, int chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

            var message = new Message
            {
                FromUserId = userId,
                Text = $"<i>{userName} has joined the chat</i>",
                Timestamp = DateTime.UtcNow,
                ChatId = chatId
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await Clients.OthersInGroup(roomName).SendAsync("UserJoined", userName);
        }

        public async Task LeaveRoom(string roomName, string userName, int userId, int chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);

            var message = new Message
            {
                FromUserId = userId,
                Text = $"<i>{userName} has left the chat</i>",
                Timestamp = DateTime.UtcNow,
                ChatId = chatId
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await Clients.OthersInGroup(roomName).SendAsync("UserLeft", userName);
        }
    }
}