using System.Linq;
using Backend.Data;
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

        [HttpPost("joinGroup/{inviteCode}")]
        public async Task<IActionResult> JoinGroup(string inviteCode)
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user.Id;

            var chat = await _context.Chats.FirstOrDefaultAsync(c => c.InviteCode == inviteCode);
            if (chat == null)
            {
                return BadRequest("Групата не беше намерена");
            }
            var isUserInChat = await _context.ChatUsers.AnyAsync(cu => cu.ChatId == chat.Id && cu.UserId == userId);
            if (isUserInChat)
            {
                return BadRequest("Вече сте член на тази група.");
            }
            var chatUser = new ChatUser
            {
                UserId = userId,
                ChatId = chat.Id
            };

            _context.ChatUsers.Add(chatUser);

            var chatEvent = new ChatEvent
            {
                UserId = userId,
                ChatId = chat.Id,
                Timestamp = DateTime.Now,
                Event = ChatEvent.EventType.UserJoined
            };

            _context.ChatEvents.Add(chatEvent);

            await _context.SaveChangesAsync();


            await _chat.Clients.Group(chat.Id.ToString()).SendAsync("UserJoined", new { user, message = chatEvent } );

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

            var chatEvent = new ChatEvent
            {
                UserId = userId,
                ChatId = chatId,
                Timestamp = DateTime.Now,
                Event = ChatEvent.EventType.UserLeft
            };

            _context.ChatEvents.Add(chatEvent);

            await _context.SaveChangesAsync();

            var group = await _context.Chats
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == chatId);

            // Send a "UserLeft" message to the clients in the group chat
            await _chat.Clients.Group(chatId.ToString()).SendAsync("UserLeft", new { user, message = chatEvent } );

            if (userId == group.CreatorId)
            {
                var newAdmin = group.Admins.OrderBy(a => Guid.NewGuid()).FirstOrDefault();

                if (newAdmin != null)
                {
                    group.CreatorId = newAdmin.UserId;
                    await _chat.Clients.Group(chatId.ToString()).SendAsync("OwnershipTransferred", newAdmin);
                }
                else
                {
                    var newUser = group.Users.OrderBy(u => Guid.NewGuid()).FirstOrDefault();

                    if (newUser != null)
                    {
                        group.CreatorId = newUser.UserId;
                        await _chat.Clients.Group(chatId.ToString()).SendAsync("OwnershipTransferred", newUser);

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

        [HttpDelete("kickUser/{chatId}/{userId}")]
        public async Task<IActionResult> KickUser(int chatId, int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return NotFound("User not found");
            }

            var chat = await _context.Chats
                .Include(c => c.Users)
                .Include(c => c.Admins)
                .FirstOrDefaultAsync(c => c.Id == chatId);

            if (chat == null)
            {
                return NotFound("Chat not found");
            }

            var chatUser = chat.Users.FirstOrDefault(cu => cu.UserId == userId);
            if (chatUser == null)
            {
                return NotFound("User not found in group");
            }

            chat.Users.Remove(chatUser);

            var chatAdmin = chat.Admins.FirstOrDefault(ca => ca.UserId == userId);
            if (chatAdmin != null)
            {
                chat.Admins.Remove(chatAdmin);
            }


            var chatEvent = new ChatEvent
            {
                UserId = userId,
                ChatId = chat.Id,
                Timestamp = DateTime.Now,
                Event = ChatEvent.EventType.UserKicked
            };

            _context.ChatEvents.Add(chatEvent);

            await _context.SaveChangesAsync();

            await _chat.Clients.Group(chatId.ToString()).SendAsync("UserKicked", new { user, message = chatEvent } );

            return Ok();
        }

        [HttpPost("promoteUser/{chatId}/{userId}")]
        public async Task<IActionResult> PromoteUser(int chatId, int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                _logger.LogError($"User with Id {userId} not found");
                return NotFound("User not found");
            }

            var chat = await _context.Chats
                .Include(c => c.Admins)
                .FirstOrDefaultAsync(c => c.Id == chatId);

            if (chat == null)
            {
                return NotFound("Chat not found");
            }

            var chatUser = chat.Users.FirstOrDefault(cu => cu.UserId == userId);
            //if (chatUser == null)
            //{
            //    return new JsonResult("User not found in group");
            //}

            var chatAdmin = new ChatAdmin
            {
                UserId = userId,
                ChatId = chat.Id
            };

            _context.ChatAdmins.Add(chatAdmin);

            var chatEvent = new ChatEvent
            {
                UserId = userId,
                ChatId = chat.Id,
                Timestamp = DateTime.Now,
                Event = ChatEvent.EventType.UserPromoted
            };

            _context.ChatEvents.Add(chatEvent);

            await _context.SaveChangesAsync();

            await _chat.Clients.Group(chatId.ToString()).SendAsync("UserPromoted", new { user, message = chatEvent });

            return Ok();
        }

        [HttpDelete("demoteUser/{chatId}/{userId}")]
        public async Task<IActionResult> DemoteUser(int chatId, int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return NotFound("User not found");
            }

            var chat = await _context.Chats
                .Include(c => c.Admins)
                .FirstOrDefaultAsync(c => c.Id == chatId);

            if (chat == null)
            {
                return NotFound("Chat not found");
            }

            var chatAdmin = chat.Admins.FirstOrDefault(ca => ca.UserId == userId);
            if (chatAdmin == null)
            {
                return NotFound("User not found in group");
            }

            chat.Admins.Remove(chatAdmin);

            var chatEvent = new ChatEvent
            {
                UserId = userId,
                ChatId = chat.Id,
                Timestamp = DateTime.Now,
                Event = ChatEvent.EventType.UserDemoted
            };

            _context.ChatEvents.Add(chatEvent);

            await _context.SaveChangesAsync();

            await _chat.Clients.Group(chatId.ToString()).SendAsync("UserDemoted",new { user, message = chatEvent } );

            return Ok();
        }

        [HttpPost("transferOwnership/{chatId}/{userId}")]
        public async Task<IActionResult> TransferOwnership(int chatId, int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return NotFound("User not found");
            }

            var chat = await _context.Chats
                .Include(c => c.Users)
                .Include(c => c.Admins)
                .FirstOrDefaultAsync(c => c.Id == chatId);

            if (chat == null)
            {
                return NotFound("Chat not found");
            }

            var chatUser = chat.Users.FirstOrDefault(cu => cu.UserId == userId);
            if (chatUser == null)
            {
                return NotFound("User not found in group");
            }

            chat.CreatorId = userId;

            var chatAdmin = chat.Admins.FirstOrDefault(ca => ca.UserId == userId);
            if (chatAdmin == null)
            {
                var newAdmin = new ChatAdmin
                {
                    UserId = userId,
                    ChatId = chat.Id
                };
                _context.ChatAdmins.Add(newAdmin);
            }

            var chatEvent = new ChatEvent
            {
                UserId = userId,
                ChatId = chat.Id,
                Timestamp = DateTime.Now,
                Event = ChatEvent.EventType.OwnershipTransferred
            };

            _context.ChatEvents.Add(chatEvent);

            await _context.SaveChangesAsync();

            await _chat.Clients.Group(chatId.ToString()).SendAsync("OwnershipTransferred", new { user, message = chatEvent} );

            return Ok();
        }



    }
}
