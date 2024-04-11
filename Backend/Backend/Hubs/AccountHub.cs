using Backend.Controllers;
using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Backend.Hubs
{

    [Route("accountHub")]
    public class AccountHub : Hub       
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountHub> _logger;
        private static readonly ConcurrentDictionary<string, string> _users = new ConcurrentDictionary<string, string>();

        public AccountHub(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<AccountHub> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext().Request.Query["userId"].ToString();
            var user = await _userManager.FindByIdAsync(userId);
            _logger.LogCritical("User connected: " + user.Id);
            if (user != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, user.Id.ToString());
                _users[user.Id.ToString()] = Context.ConnectionId;
                _logger.LogInformation($"User connected: {user.Id}, Connection ID: {Context.ConnectionId}");
            }

            await base.OnConnectedAsync();
        }


        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var user = await _userManager.GetUserAsync(Context.User);
            if (user != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, user.Id.ToString());
                _users.TryRemove(user.Id.ToString(), out _); // Remove the user's ID

                user.Status = "Offline";
                await _userManager.UpdateAsync(user);

                await Clients.All.SendAsync("UpdateUserStatus", user.Id, "offline");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public static string GetConnectionIdForUser(string userId)
        {
            _users.TryGetValue(userId, out string connectionId);
            return connectionId;
        }
        public async Task UpdateUserStatus(string userId, string status)
        {
            if (userId == null || status == null)
            {
                return;
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("User not found with id: " + userId);
                return;
            }

            user.Status = status;
            await _context.SaveChangesAsync();

            await Clients.All.SendAsync("UpdateUserStatus", userId, status);
        }

    }
}
