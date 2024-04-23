namespace Backend.Models.InputModels
{
    public class UpdateUserInput
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Birthdate { get; set; }
        public string Pronouns { get; set; }
    }
}
