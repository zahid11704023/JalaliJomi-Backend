using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using JalaliJomi.Backend.Data;
using JalaliJomi.Backend.Models;
using JalaliJomi.Backend.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace JalaliJomi.Backend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<RegisteredUser> _userManager;
        private readonly SignInManager<RegisteredUser> _signInManager;
        private readonly AppDbContext _context;

        public AuthController(
            UserManager<RegisteredUser> userManager,
            SignInManager<RegisteredUser> signInManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest(new[] { new { code = "DuplicateEmail", description = $"Email '{dto.Email}' is already taken." } });

            var user = new RegisteredUser
            {
                UserName = dto.Email, // Identity requires a UserName; we use the email as username
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.Phone,
                RegistrationDate = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors); // default Identity error format: [{ code, description }]

            // Auto-login after registration (sets the HttpOnly auth cookie), matching Manuel's current UX flow
            await _signInManager.SignInAsync(user, isPersistent: true);

            var userDto = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                Role = "buyer" // freshly registered users can't own listings yet
            };

            return Ok(userDto);
        }

        // --- next up: Login, Logout, Me ---

        [HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginDto dto)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    var user = await _userManager.FindByEmailAsync(dto.Email);
    if (user == null)
        return Unauthorized(new { error = "Invalid email or password." });

    var result = await _signInManager.PasswordSignInAsync(user, dto.Password, isPersistent: true, lockoutOnFailure: false);

    if (!result.Succeeded)
        return Unauthorized(new { error = "Invalid email or password." });

    var isOwner = await _context.Listings.AnyAsync(l => l.PropertyOwnerId == user.Id);

    var userDto = new UserDto
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email!,
        Role = isOwner ? "owner" : "buyer"
    };

    return Ok(userDto);
}

        [HttpPost("logout")]
public async Task<IActionResult> Logout()
{
    await _signInManager.SignOutAsync();
    return Ok(new { message = "Logged out successfully." });
}

        [HttpGet("me")]
        public Task<IActionResult> Me()
        {
            throw new NotImplementedException(); // TODO next step — role computed by checking Listings table
        }
    }
}