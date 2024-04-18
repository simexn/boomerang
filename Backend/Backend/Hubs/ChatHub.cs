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
        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }
    }
}