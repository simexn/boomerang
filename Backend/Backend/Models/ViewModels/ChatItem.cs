namespace Backend.Models.ViewModels
{
    public class ChatItem
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; }
        public string EventType { get; set; }
    }
}
