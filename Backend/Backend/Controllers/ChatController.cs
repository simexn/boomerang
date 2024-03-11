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
        public async Task<IActionResult> GetMessages([FromQuery] string chatId)
        {
            var messages = await _context.Messages.Where(x => x.ChatId == Int32.Parse(chatId))
                .Include(x => x.FromUser)
                .ToListAsync();

            return new JsonResult(new { messages });
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
                Timestamp = DateTime.Now
            };

            _context.Messages.Add(Message);
            await _context.SaveChangesAsync();

            await _chat.Clients.Group(requestBody[2]).SendAsync("ReceiveMessage", Message);
            return Ok();
        }

    }
}
