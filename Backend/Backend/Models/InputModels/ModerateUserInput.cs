namespace Backend.Models.InputModels
{
    public class ModerateUserInput
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string? NewPassword { get; set; }
        public string BirthDate { get; set; }
        public string Pronouns { get; set; }
        public string ProfilePictureUrl { get; set; }
        public bool IsAdmin { get; set; }
    }
}
