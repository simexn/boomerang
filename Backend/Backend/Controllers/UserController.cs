using Backend.Data;
using Backend.Hubs;
using Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;

namespace Backend.Controllers
{
    [Route("user")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private IHubContext<ChatHub> _chat;
        private IHubContext<AccountHub> _user;

        public UserController(ILogger<ChatController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHubContext<AccountHub> user, IHubContext<ChatHub> chat)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _user = user;
            _chat = chat;
        }

        [HttpPost("addFriend/{username}")]
        public async Task<IActionResult> AddFriend(string username)
        {
            var user = await _userManager.GetUserAsync(User);
            var friend = await _userManager.FindByNameAsync(username);
            if (friend == null)
            {
                return NotFound("User with that username not found.");
            }

            var doesFriendshipExist = await _context.Friendships.FirstOrDefaultAsync(f => (f.UserId == user.Id && f.FriendId == friend.Id) || (f.FriendId == user.Id && f.UserId == friend.Id));

            if (doesFriendshipExist != null)
            {
                if (doesFriendshipExist.Status == "pending")
                {
                    return new JsonResult(new { friendRequestPending = true });
                }
                else if (doesFriendshipExist.Status == "accepted")
                {
                    return new JsonResult(new { alreadyFriends = true });
                }
                else if (doesFriendshipExist.Status == "blocked")
                {
                    return new JsonResult(new { userBlocked = true });
                }
            }

            var friendship = new Friendship
            {
                UserId = user.Id,
                FriendId = friend.Id,
                Status = "pending",
                RequestSentDate = DateTime.UtcNow,
                RequestRespondedDate = DateTime.UtcNow
            };

            _context.Friendships.Add(friendship);
            await _context.SaveChangesAsync();

            var friendConnectionId = AccountHub.GetConnectionIdForUser(friend.Id.ToString());
            _logger.LogCritical($"Friend connection ID: {friendConnectionId}");
            if (!string.IsNullOrEmpty(friendConnectionId))
            {
                await _user.Clients.Client(friendConnectionId).SendAsync("NewFriendRequest", new { username = user.UserName, userPfp=user.ProfilePictureUrl, requestSentDate = friendship.RequestSentDate.ToString("dd/MM/yyyy HH:mm") });
            }




            return new JsonResult(new {id = friend.Id, username = friend.UserName, userPfp = friend.ProfilePictureUrl, requestSentDate = friendship.RequestSentDate.ToString("dd/MM/yyyy HH:mm") });

        }

        [HttpGet("getFriendRequests")]
        public async Task<ActionResult> GetFriendRequests()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var sentRequests = await _context.Friendships
                .Where(f => f.UserId == user.Id && f.Status == "Pending")
                .Select(f => new
                {
                    userId = f.Friend.Id,
                    username = f.Friend.UserName,
                    profilePictureUrl = f.Friend.ProfilePictureUrl,
                    requestSentDate = f.RequestSentDate.ToString("dd/MM/yyyy HH:mm")
                })
                .ToListAsync();

            var receivedRequests = await _context.Friendships
                .Where(f => f.FriendId == user.Id && f.Status == "Pending")
                .Select(f => new
                {
                    userId = f.User.Id,
                    username = f.User.UserName,
                    profilePictureUrl = f.User.ProfilePictureUrl,
                    requestSentDate = f.RequestSentDate.ToString("dd/MM/yyyy HH:mm")
                })
                .ToListAsync();

            return new JsonResult(new { sentRequests, receivedRequests });
        }
        [HttpGet("getFriends")]
        public async Task<IActionResult> GetFriends()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            var friends = await _context.Friendships
                .Where(f => (f.UserId == user.Id || f.FriendId == user.Id) && f.Status == "Accepted")
                .Select(f => new {
                    id = f.UserId == user.Id ? f.Friend.Id : f.User.Id,
                    username = f.UserId == user.Id ? f.Friend.UserName : f.User.UserName,
                    profilePictureUrl = f.UserId == user.Id ? f.Friend.ProfilePictureUrl : f.User.ProfilePictureUrl,
                    chatId = _context.ChatUsers
                        .Where(cu => cu.UserId == user.Id && _context.ChatUsers.Any(cu2 => cu2.ChatId == cu.ChatId && cu2.UserId == (f.UserId == user.Id ? f.Friend.Id : f.User.Id)))
                        .Select(cu => cu.ChatId)
                        .FirstOrDefault(cId => _context.Chats.Any(c => c.Id == cId && c.IsGroup == false))
                            })
                .ToListAsync();
            return new JsonResult(new { friends });
        }

        [HttpGet("getFriendInfo/{chatId}")]
        public async Task<IActionResult> GetFriendInfo(int chatId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var chat = await _context.Chats
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == chatId && c.Users.Any(u => u.UserId == user.Id));

            if (chat == null)
            {
                return BadRequest();
            }

            var friendship = await _context.Friendships
            .FirstOrDefaultAsync(f => f.ChatId == chatId && (f.UserId == user.Id || f.FriendId == user.Id));

            if (friendship == null)
            {
                return BadRequest("Friendship not found for the chat");
            }

            var friendUserId = friendship.UserId == user.Id ? friendship.FriendId : friendship.UserId;

            var mutualFriendsData = await _context.Friendships
            .Where(f => f.Status == "Accepted" && (f.UserId == user.Id || f.FriendId == user.Id)) // friendships involving the user
            .Select(f => f.UserId == user.Id ? f.FriendId : f.UserId) // get the friend's id
            .Intersect( // find common friends
                _context.Friendships
            .Where(f => f.Status == "Accepted" && (f.UserId == friendUserId || f.FriendId == friendUserId)) // friendships involving the friend
            .Select(f => f.UserId == friendUserId ? f.FriendId : f.UserId) // get the friend's friend id
            ).ToListAsync();

            var mutualFriends = await _context.Users
            .Where(u => mutualFriendsData.Contains(u.Id))
            .Select(u => new
            {
                Id = u.Id,
                Username = u.UserName,
                userPfp = u.ProfilePictureUrl,
                ChatId = u.Id == user.Id ? chatId : _context.Friendships
                    .Where(f => f.Status == "Accepted" && (f.UserId == user.Id || f.FriendId == user.Id) && (f.UserId == u.Id || f.FriendId == u.Id))
                    .Select(f => f.ChatId)
                    .FirstOrDefault()
            })
            .ToListAsync();

            var mutualServers = await _context.Chats
            .Where(c => _context.ChatUsers.Any(cu => cu.ChatId == c.Id && cu.UserId == user.Id) &&
                        _context.ChatUsers.Any(cu => cu.ChatId == c.Id && cu.UserId == friendUserId) && c.IsGroup == true)
            .Select(c => new
            {
                Id = c.Id,
                Name = c.Name,
                // Add other properties as needed
            })
            .ToListAsync();

            var friend = await _context.ChatUsers
            .Where(cu => cu.ChatId == chatId && cu.UserId != user.Id)
            .Select(cu => new
            {
                id = cu.UserId,
                username = cu.User.UserName,
                userPfp = cu.User.ProfilePictureUrl,
                memberSince = cu.User.AccountCreatedDate.ToString("MMM d, yyyy"),
                friendsSince = _context.Friendships
                    .Where(f => (f.UserId == user.Id && f.FriendId == cu.UserId) || (f.UserId == cu.UserId && f.FriendId == user.Id))
                    .Select(f => f.RequestRespondedDate)
                    .FirstOrDefault()
                    .ToString("MMM d, yyyy"),
                mutualFriends,
                mutualGroups = mutualServers
                
            })
            .FirstOrDefaultAsync();


            return new JsonResult(new { friend });
        }


        [HttpPost("acceptFriendRequest/{username}")]
        public async Task<ActionResult> AcceptRequest(string username)
        {
            var user = await _userManager.GetUserAsync(User);
            var friend = await _userManager.FindByNameAsync(username);

            if (friend == null)
            {
                return new JsonResult(new { friendNotFound = true });
            }

            var friendship = await _context.Friendships.FirstOrDefaultAsync(f => f.UserId == friend.Id && f.FriendId == user.Id && f.Status == "Pending");

            if (friendship == null)
            {
                return new JsonResult(new { requestNotFound = true });
            }

            friendship.Status = "Accepted";
            friendship.RequestRespondedDate = DateTime.UtcNow;

            var chat = new Chat
            {
                Name = $"{user.UserName} and {friend.UserName}",
                IsGroup = false,
                CreatorId = user.Id
            };
            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            // Set the ChatId in the Friendship entity
            friendship.ChatId = chat.Id;
            _context.Friendships.Update(friendship);
            await _context.SaveChangesAsync();

            // Add the users to the chat
            var chatUser1 = new ChatUser { UserId = user.Id, ChatId = chat.Id };
            var chatUser2 = new ChatUser { UserId = friend.Id, ChatId = chat.Id };
            _context.ChatUsers.AddRange(chatUser1, chatUser2);
            await _context.SaveChangesAsync();

            var friendConnectionId = AccountHub.GetConnectionIdForUser(friend.Id.ToString());
            await _user.Clients.Client(friendConnectionId).SendAsync("FriendRequestAccepted", new { user, friend, chatId = chat.Id });

            var userConnectionId = AccountHub.GetConnectionIdForUser(user.Id.ToString());
            await _user.Clients.Client(userConnectionId).SendAsync("FriendRequestAccepted", new { user, friend, chatId = chat.Id });

            return new JsonResult(new { requestAccepted = true });
        }


        [HttpDelete("rejectFriendRequest/{username}")]
        public async Task<ActionResult> RejectRequest(string username)
        {
            var user = await _userManager.GetUserAsync(User);
            var friend = await _userManager.FindByNameAsync(username);

            if (friend == null)
            {
                return new JsonResult(new { friendNotFound = true });
            }

            var friendship = await _context.Friendships.FirstOrDefaultAsync(f => f.UserId == friend.Id && f.FriendId == user.Id && f.Status == "Pending");

            if (friendship == null)
            {
                return new JsonResult(new { requestNotFound = true });
            }

            _context.Friendships.Remove(friendship);
            await _context.SaveChangesAsync();

            var friendConnectionId = AccountHub.GetConnectionIdForUser(friend.Id.ToString());
            await _user.Clients.Client(friendConnectionId).SendAsync("FriendRequestRejected", new {user, friend});

            var userConnectionId = AccountHub.GetConnectionIdForUser(user.Id.ToString());
            await _user.Clients.Client(userConnectionId).SendAsync("FriendRequestRejected", new {user, friend});

            return new JsonResult(new { requestRejected = true });
        }

        [HttpDelete("removeFriend/{friendId}")]
        public async Task<IActionResult> RemoveFriend(string friendId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var friend = await _userManager.FindByIdAsync(friendId);
            if (friend == null)
            {
                return NotFound(new { message = "Friend not found" });
            }

            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f => (f.UserId == user.Id && f.FriendId == friend.Id) || (f.UserId == friend.Id && f.FriendId == user.Id));

            if (friendship == null)
            {
                return NotFound(new { message = "Friendship not found" });
            }

            var chatId = friendship.ChatId;

            var chat = await _context.Chats.FirstOrDefaultAsync(c => c.Id == friendship.ChatId);
            _context.Chats.Remove(chat);
            await _context.SaveChangesAsync();

            _context.Friendships.Remove(friendship);
            await _context.SaveChangesAsync();

            await _chat.Clients.Groups(chatId.ToString()).SendAsync("FriendRemoved");

            var friendConnectionId = AccountHub.GetConnectionIdForUser(friend.Id.ToString());
            await _user.Clients.Client(friendConnectionId).SendAsync("FriendRemoved", new { user, friend, chatId = friendship.ChatId });

            var userConnectionId = AccountHub.GetConnectionIdForUser(user.Id.ToString());
            await _user.Clients.Client(userConnectionId).SendAsync("FriendRemoved", new { user, friend, chatId = friendship.ChatId});

            

            return Ok(new { message = "Friend removed successfully" });
        }
    }
}
