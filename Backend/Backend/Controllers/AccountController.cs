using Backend.Models.InputModels;
using Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Backend.Services;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace Backend.Controllers
{
    [ApiController]
    [Route("account")]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly ITokenService _tokenService;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<AuthenticationController> logger
        , ITokenService tokenService, ApplicationDbContext context)
        {
            _tokenService = tokenService;
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _context = context;
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
                user.AccountCreatedDate,
                user.Birthdate,
                user.Pronouns,
                user.ProfilePictureUrl,
                user.IsAdmin
                
            };

            return new JsonResult(new { userInfo });
        }
        [HttpGet("getUserId")]
        public async Task<IActionResult> GetUserId()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            return new JsonResult(new { userId = user.Id });
        }

        [HttpGet("getActiveUsers")]
        public async Task<IActionResult> GetActiveUsers()
        {
            var users = await _context.Users.ToListAsync();
            var activeUsers = users.ToDictionary(u => u.Id.ToString(), u => u.Status);

            return new JsonResult(new { activeUsers });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterInput model)
        {
            var existingUser = await _userManager.FindByNameAsync(model.UserName); 
            if (existingUser != null)
            {
                return new JsonResult ( new {userExists=true});
            }

            existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return new JsonResult ( new {emailExists=true});
            }

            _logger.LogInformation(model.Password);
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,                
                ProfilePictureUrl = $"/images/profile_pictures/placeholder.png",
                AccountCreatedDate = DateTime.UtcNow,
                Birthdate = model.Birthdate,
                Pronouns = model.Pronouns,
                Status = "offline"
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

            return new JsonResult(new { accountRegistered = true });
        }

        [HttpPost("uploadPfp")]
        public async Task<IActionResult> UploadPfp([FromForm] IFormFile file)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var filePath = Path.Combine("wwwroot", "images", "profile_pictures", user.Id + ".png");
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Add a version parameter to the URL based on the current timestamp
            var version = DateTime.UtcNow.Ticks;
            user.ProfilePictureUrl = $"/images/profile_pictures/{user.Id}.png?v={version}";

            await _userManager.UpdateAsync(user);

            return new JsonResult(new { pfpUploaded = true });
        }

        [HttpPost("updateInformation")]
        public async Task<IActionResult> UpdateInformation([FromBody] UpdateUserInput model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            // Validate the input model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Update the user's information
            user.UserName = model.Username;
            user.Email = model.Email;
            user.Birthdate = model.Birthdate;
            user.Pronouns = model.Pronouns;
            if (!string.IsNullOrEmpty(model.Password))
            {
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, model.Password);
            }

            // Add other fields as needed

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return new JsonResult(new { accountUpdated = true });
        }
    }


}
