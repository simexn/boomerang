using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;

namespace Backend.Hubs
{
    [Route("chatHub")]
    public class ChatHub : Hub
    {
        //generate chat hub
        public ChatHub() {

        
        }

        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }
    }
}
