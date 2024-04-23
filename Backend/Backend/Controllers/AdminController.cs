using Backend.Data;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("admin")]
    public class AdminController : Controller
    {
        private ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AuthenticationController> _logger;


        public AdminController(UserManager<ApplicationUser> userManager, ILogger<AuthenticationController> logger, ApplicationDbContext context)
        {
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }

        [HttpGet("getUserInfo/{input}")]
        public async Task<IActionResult> FindUser(string input)
        {
            int id;
            var user = int.TryParse(input, out id) ? await _userManager.FindByIdAsync(input) : null;
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(input);
                if (user == null)
                {
                    user = await _userManager.FindByNameAsync(input);
                    if (user == null)
                    {
                        return NotFound();
                    }
                }
            }


            var userInfo = new
            {
                id = user.Id,
                username = user.UserName,
                email = user.Email,
                accountCreatedDate = user.AccountCreatedDate,
                birthDate = user.Birthdate,
                pronouns = user.Pronouns,
                profilePictureUrl = user.ProfilePictureUrl,
                IsAdmin = user.IsAdmin
            };

            return new JsonResult(new { userInfo });
        }
    }
}
