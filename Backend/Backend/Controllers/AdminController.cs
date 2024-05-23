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
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly ITokenService _tokenService;

        public AdminController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<AuthenticationController> logger
        , ITokenService tokenService, ApplicationDbContext context)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }

        [HttpGet("getUserModerate/{identificator}")]
        public async Task<IActionResult> GetUser(string identificator)
        {
            var user = await _userManager.FindByNameAsync(identificator);
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(identificator);
                if (user == null)
                {
                    user = await _userManager.FindByIdAsync(identificator);
                    if (user == null)
                    {
                        return NotFound("Потребителят не беше намерен.");
                    }
                }
            }

            return new JsonResult(new
            {
                id = user.Id,
                username = user.UserName,
                email = user.Email,
                profilePictureUrl = user.ProfilePictureUrl,
                accountCreatedDate = user.AccountCreatedDate,
                birthDate = user.BirthDate,
                pronouns = user.Pronouns,
                isAdmin = user.IsAdmin
            });


        }

        [HttpPost("updateUser")]
        public async Task<IActionResult> UpdateUser([FromBody] ModerateUserInput model)
        {
            var user = await _userManager.FindByIdAsync(model.Id.ToString());
            _logger.LogCritical("User id: " + model.Id);
            if (user == null)
            {
                return NotFound("Потребителят не беше намерен.");
            }

            user.UserName = model.Username;
            user.Email = model.Email;
            user.BirthDate = model.BirthDate;
            user.Pronouns = model.Pronouns;
            user.IsAdmin = model.IsAdmin;
            user.ProfilePictureUrl = model.ProfilePictureUrl;

            if (model.NewPassword != null || model.NewPassword != "" || model.NewPassword.Length !< 6)
            {
                _logger.LogCritical("New password: " + model.NewPassword);
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, model.NewPassword);
            }

            await _userManager.UpdateAsync(user);

            return Ok();
        }

        [HttpPost("deleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound("Потребителят не беше намерен.");
            }

            user.UserName = "Deleted User";
            user.Email = "deleted@user.com";

            await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
