namespace Backend.Models.InputModels
{
    public class RegisterInput
    {
        public string UserName {  get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Pronouns { get; set; }
        public string BirthDate { get; set; }

    }
}
