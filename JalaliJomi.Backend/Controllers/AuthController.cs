using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
                return BadRequest(ModelStateToFieldErrors());

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest(new ErrorResponseDto
                {
                    Errors = new Dictionary<string, string[]> { ["email"] = new[] { "Email already in use" } }
                });

            var user = new RegisteredUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.Phone,
                RegistrationDate = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(BuildFieldErrors(result.Errors));

            await _signInManager.SignInAsync(user, isPersistent: true);

            return Ok(new UserDto
            {
                Id = user.Id,
                Name = user.FullName,
                Email = user.Email!,
                Phone = user.PhoneNumber ?? string.Empty,
                Role = "buyer"
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelStateToFieldErrors());

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Unauthorized(new ErrorResponseDto { Error = "Invalid email or password." });

            var result = await _signInManager.PasswordSignInAsync(user, dto.Password, isPersistent: true, lockoutOnFailure: false);

            if (!result.Succeeded)
                return Unauthorized(new ErrorResponseDto { Error = "Invalid email or password." });

            var isOwner = await _context.Listings.AnyAsync(l => l.PropertyOwnerId == user.Id);

            return Ok(new UserDto
            {
                Id = user.Id,
                Name = user.FullName,
                Email = user.Email!,
                Phone = user.PhoneNumber ?? string.Empty,
                Role = isOwner ? "owner" : "buyer"
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logged out successfully." });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized(new ErrorResponseDto { Error = "Not logged in." });

            var isOwner = await _context.Listings.AnyAsync(l => l.PropertyOwnerId == user.Id);

            return Ok(new UserDto
            {
                Id = user.Id,
                Name = user.FullName,
                Email = user.Email!,
                Phone = user.PhoneNumber ?? string.Empty,
                Role = isOwner ? "owner" : "buyer"
            });
        }

        // --- private helper methods ---

        private ErrorResponseDto BuildFieldErrors(IEnumerable<IdentityError> errors)
        {
            var fieldErrors = new Dictionary<string, List<string>>();
            var generalErrors = new List<string>();

            foreach (var error in errors)
            {
                if (error.Code.Contains("Email", StringComparison.OrdinalIgnoreCase))
                    AddFieldError(fieldErrors, "email", error.Description);
                else if (error.Code.Contains("Password", StringComparison.OrdinalIgnoreCase))
                    AddFieldError(fieldErrors, "password", error.Description);
                else
                    generalErrors.Add(error.Description);
            }

            return new ErrorResponseDto
            {
                Errors = fieldErrors.Count > 0 ? fieldErrors.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToArray()) : null,
                Error = generalErrors.Count > 0 ? string.Join(" ", generalErrors) : null
            };
        }

        private void AddFieldError(Dictionary<string, List<string>> dict, string field, string message)
        {
            if (!dict.ContainsKey(field)) dict[field] = new List<string>();
            dict[field].Add(message);
        }

        private ErrorResponseDto ModelStateToFieldErrors()
        {
            var errors = ModelState
                .Where(kvp => kvp.Value!.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key.ToLower(),
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            return new ErrorResponseDto { Errors = errors };
        }
    }
}