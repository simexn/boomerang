namespace Backend.Models.ViewModels
{
    public class ChatItem
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }
        public bool IsEvent { get; set; }
        public string IsActive { get; set; }
        public string EventType { get; set; }
        public string UserPfp { get; set; }
        
        public bool IsEdited { get; set; }
        public bool IsDeleted { get; set; }
        public bool WithDetails { get; set; }
    }
}
