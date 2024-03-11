using Backend.Models.InputModels;
using Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Backend.Services;

namespace Backend.Controllers
{
    [ApiController]
    [Route("account")]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly ITokenService _tokenService;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<AuthenticationController> logger
        , ITokenService tokenService)
        {
            _tokenService = tokenService;
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("getUserInfo")]
        public async Task<IActionResult> GetUserInfo()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var userInfo = new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.AccountCreatedDate
                
            };

            return new JsonResult(new { userInfo });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterInput model)
        {
            _logger.LogInformation(model.Password);
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,                
                ProfilePictureUrl = null,
                AccountCreatedDate = DateTime.UtcNow,
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var token = await _tokenService.GenerateToken(user);
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(365)
            };
            Response.Cookies.Append("token", token, cookieOptions);

            return new JsonResult(new { token });
        }
    }


}
