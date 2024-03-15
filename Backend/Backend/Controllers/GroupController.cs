﻿using Backend.Data;
using Backend.Hubs;
using Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("group")]
    public class GroupController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private IHubContext<ChatHub> _chat;

        public GroupController(ILogger<ChatController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHubContext<ChatHub> chat)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _chat = chat;
        }

        [HttpPost("joinGroup")]
        public async Task<IActionResult> JoinGroup([FromBody] string inviteCode)
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            var chat = _context.Chats.FirstOrDefault(c => c.InviteCode == inviteCode);
            if (chat == null)
            {
                return BadRequest("Chat not found");
            }
            var chatUser = new ChatUser
            {
                UserId = userId,
                ChatId = chat.Id
            };

            _context.ChatUsers.Add(chatUser);
            await _context.SaveChangesAsync();

            return new JsonResult(new { chat });
           
        }


        [HttpDelete("leaveGroup/{chatId}")]
        public async Task<IActionResult> LeaveGroup(int chatId)
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            var chatUser = await _context.ChatUsers.FirstOrDefaultAsync(cu => cu.ChatId == chatId && cu.UserId == userId);
            if (chatUser == null)
            {
                return NotFound("User not found in group");
            }

            _context.ChatUsers.Remove(chatUser);

            var chatAdmin = await _context.ChatAdmins.FirstOrDefaultAsync(ca => ca.ChatId == chatId && ca.UserId == userId);
            if (chatAdmin != null)
            {
                _context.ChatAdmins.Remove(chatAdmin);
            }

            
            await _context.SaveChangesAsync();

            var group = await _context.Chats
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == chatId);



            if (userId == group.CreatorId)
            {
                var newAdmin = group.Admins.OrderBy(a => Guid.NewGuid()).FirstOrDefault();

                if (newAdmin != null)
                {
                    group.CreatorId = newAdmin.UserId;
                }
                else
                {
                    var newUser = group.Users.OrderBy(u => Guid.NewGuid()).FirstOrDefault();

                    if (newUser != null)
                    {
                        group.CreatorId = newUser.UserId;

                        // Add the new creator to the group's admins
                        var newAdminUser = new ChatAdmin
                        {
                            UserId = newUser.UserId,
                            ChatId = group.Id
                        };
                        _context.ChatAdmins.Add(newAdminUser);
                    }
                    else
                    {
                        if (group.Users.Count == 0)
                        {
                            _context.Chats.Remove(group);
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                await _context.SaveChangesAsync();
            }

            return Ok();
        }


            [HttpDelete("deleteGroup/{chatId}")]
        public async Task<IActionResult> DeleteGroup(int chatId)
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            var chat = await _context.Chats
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == chatId);

            if (chat == null)
            {
                return NotFound("Chat not found");
            }

            if (userId != chat.CreatorId)
            {
                return Unauthorized();
            }

            _context.Chats.Remove(chat);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("getGroupInfo")]
        public async Task<IActionResult> GetGroupInfo([FromQuery] int chatId)
        {
            var chat = await _context.Chats
                .Include(c => c.Users)
                .ThenInclude(cu => cu.User)
                .Include(c => c.Admins)
                .ThenInclude(ca => ca.User)
                .FirstOrDefaultAsync(c => c.Id == chatId);

            if (chat == null)
            {
                return NotFound("Chat not found");
            }

            var users = chat.Users.Select(cu => cu.User).ToList();
            var admins = chat.Admins.Select(ca => ca.User).ToList();

            return new JsonResult(new { chat, users, admins });
        }

        [HttpGet("getGroupUsers")]
        public async Task<IActionResult> GetGroupUsers([FromQuery] int chatId)
        {
            var chatUsers = await _context.ChatUsers
                .Include(cu => cu.User)
                .Where(cu => cu.ChatId == chatId)
                .ToListAsync();

            if (!chatUsers.Any())
            {
                return NotFound("Chat not found");
            }

            var users = chatUsers.Select(cu => cu.User).ToList();

            return new JsonResult(new { users });
        }

    }
}
