using Backend.Controllers;
using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Hubs
{
    [Route("accountHub")]
    public class AccountHub : Hub       
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountHub> _logger;

        public AccountHub(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<AccountHub> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }

        public async Task UpdateUserStatus(string userId, string status)
        {
          _logger.LogCritical("UpdateUserStatus method accessed.");
            _logger.LogInformation("Userid: " + userId);
            var user = await _userManager.FindByIdAsync(userId);
            _logger.LogInformation("User: " + user);
            user.Status = status;
            await _context.SaveChangesAsync();
            if (userId == null || status == null)
            {
                return;
            }

            await Clients.Others.SendAsync("UpdateUserStatus", userId, status);
        }
    }
}
