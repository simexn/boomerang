using Backend.Data;
using Backend.Hubs;
using Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Backend.Models.InputModels;
using System.Diagnostics;
using System.Text.Json;
using Backend.Models.ViewModels;

namespace Backend.Controllers
{
    [ApiController]
    [Route("chat")]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private IHubContext<ChatHub> _chat;

        public ChatController(ILogger<ChatController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHubContext<ChatHub> chat)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _chat = chat;
        }

        private async Task<bool> IsUserInChat(int userId, int chatId)
        {
            return await _context.ChatUsers.AnyAsync(cu => cu.UserId == userId && cu.ChatId == chatId);
        }

        [HttpGet("getAllChats")]
        public async Task<IActionResult> GetAllChats()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized("User is either not logged in or an error is present with the user object");

            var chats = await _context.Chats.Where(c => c.Users.Any(u => u.UserId == user.Id)).ToListAsync();
            if (chats == null) return BadRequest("No chats found for this user");

            return new JsonResult(new { chats });

        }


        [HttpPost("createRoom")]
        public async Task<IActionResult> CreateRoom([FromBody] CreateChatInput createChatInput)
        {
            var user = await _userManager.GetUserAsync(User);

            var chat = new Chat
            {
                Name = createChatInput.NewGroupName,
                InviteCode = createChatInput.InviteCode,
                CreatorId = user.Id


            };

            await _context.Chats.AddAsync(chat);
            await _context.SaveChangesAsync();

            chat.Users.Add(new ChatUser
            {
                UserId = user.Id,
                ChatId = chat.Id
            }
            );
            chat.Admins.Add(new ChatAdmin
            {
                UserId = user.Id,
                ChatId = chat.Id
            }
            );

            _context.Update(chat);
            await _context.SaveChangesAsync();

            return Ok("Room Created");
        }

        public IActionResult CreateMessage(int id)
        {
            var chat = _context.Chats
                .Include(x => x.Messages)
                .FirstOrDefault(x => x.Id == id);
                
                return Ok(chat);
        }

        

        [HttpPost("createMessage")]
        public async Task<IActionResult> CreateMessage([FromBody] string[] requestBody)
        {

            var user = await _userManager.GetUserAsync(User);

            var message = new Message
            {
                ChatId = Int32.Parse(requestBody[1]),
                Text = requestBody[0],
                FromUserId = user.Id,
                Timestamp = DateTime.Now
            };

            await _context.Messages.AddAsync(message);

            await _context.SaveChangesAsync();
            return Ok("ok");
          
        }



        [HttpGet("getMessages")]
        public async Task<IActionResult> GetMessages([FromQuery] int chatId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || !(await IsUserInChat(user.Id, chatId)))
            {
                return Unauthorized();
            }

            var messages = await _context.Messages
                .Where(m => m.ChatId == chatId)
                .Select(m => new ChatItem
                {
                    Id = m.Id,
                    Content = m.Text,
                    Timestamp = m.Timestamp,
                    UserName = m.FromUser.UserName,
                    IsEdited = m.IsEdited,
                    IsDeleted = m.IsDeleted
                    
                })
                .ToListAsync();

            var events = await _context.ChatEvents
                .Where(e => e.ChatId == chatId)
                .Select(e => new ChatItem
                {
                    Id = e.Id,
                    Content = e.Event.ToString(),
                    Timestamp = e.Timestamp,
                    UserName = e.User.UserName,
                    EventType = e.Event.ToString(),
                    IsEvent = true
                })
                .ToListAsync();

            var chatItems = messages.Concat(events).OrderBy(i => i.Timestamp).ToList();

            return new JsonResult(new { chatItems });
        }



        [HttpPost("joinRoom")]
        public async Task<IActionResult> JoinRoom([FromBody] JoinRoomInput request)
        {
            _logger.LogInformation("Joining room with id {0} with connection ID {1}", request.RoomName, request.ConnectionId);
            await _chat.Groups.AddToGroupAsync(request.ConnectionId, request.RoomName);
            return Ok();
        }

        public async Task<IActionResult> LeaveRoom(string connectionId, string roomId)
        {
            await _chat.Groups.RemoveFromGroupAsync(connectionId, roomId);
            return Ok();
        }

        [HttpPost("sendMessage")]
        public async Task<IActionResult> SendMessageToRoom([FromBody] string[] requestBody)
        {
            var user = await _userManager.GetUserAsync(User);

            var Message = new Message
            {
                ChatId = Int32.Parse(requestBody[1]),
                Text = requestBody[0],
                FromUserId = user.Id,
                Timestamp = DateTime.Now,
                IsEdited = false,
                IsDeleted = false
                
            };

            _context.Messages.Add(Message);
            await _context.SaveChangesAsync();

            await _chat.Clients.Group(requestBody[2]).SendAsync("ReceiveMessage", Message);
            return Ok();
        }

        [HttpPost("editMessage/{chatId}/{messageId}/{newContent}")]
        public async Task<IActionResult> EditMessage(string chatId, string messageId, string newContent)
        {
            var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == Int32.Parse(messageId));
            message.Text = newContent;
            message.IsEdited = true;
            _context.Update(message);
            await _context.SaveChangesAsync();

            await _chat.Clients.Group(chatId).SendAsync("EditedMessage", message);
            return Ok();
        }

        [HttpPost("deleteMessage/{chatId}/{messageId}")]
        public async Task<IActionResult> DeleteMessage(string chatId, string messageId)
        {
            var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == Int32.Parse(messageId));
            message.IsDeleted = true;
            _context.Update(message);
            await _context.SaveChangesAsync();

            await _chat.Clients.Group(chatId).SendAsync("DeletedMessage", message);
            return Ok();
        }

    }
}
