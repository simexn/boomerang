using Backend.Models;

namespace Backend.Services
{
    public interface ITokenService
    {
        Task<string> GenerateToken(ApplicationUser user);
    }
}