using EduSyncAPI.Data;
using edusync_api.Model.Dto;
using EduSyncAPI.Dto;
using EduSyncAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using EduSyncAPI.Services;
using System.Text;

namespace EduSync.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly EmailService _emailService;
        private readonly EduSyncDbContext _context;

        //public AuthController(EduSyncDbContext context)
        //{
        //    _context = context;
        //}
        public AuthController(EmailService emailService, EduSyncDbContext context)
        {
            _emailService = emailService;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto userDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
                return BadRequest("User already exists");

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = userDto.Name,
                Email = userDto.Email,
                Role = userDto.Role,
                //PasswordHash = ComputeSha256Hash(userDto.PasswordHash)
               PasswordHash = ComputeSha256Hash(userDto.Password)

            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || user.PasswordHash != ComputeSha256Hash(request.Password))
                return Unauthorized("Invalid credentials");

            return Ok(new
            {
                message = "Login successful",
                userId = user.UserId,
                name = user.Name,
                email = user.Email,
                role = user.Role
            });
        }
        // In Controllers/AuthController.cs

        [HttpPost("forget-password")]
        public IActionResult ForgetPassword([FromBody] ForgetPasswordDto dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email);
            if (user == null)
                return NotFound("User not found");

            // Generate a token (for demo, use a GUID)
            var token = Guid.NewGuid().ToString();
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            _context.SaveChanges();

            var resetLink = $"http://localhost:5173/reset-password?email={user.Email}&token={token}";
            _emailService.SendEmail(user.Email, "Password Reset", $"Reset your password using this link: {resetLink}");

            return Ok("Password reset link sent to your email.");
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email && u.PasswordResetToken == dto.Token && u.PasswordResetTokenExpiry > DateTime.UtcNow);
            if (user == null)
                return BadRequest("Invalid or expired token.");

            // Hash the password in production!
            user.PasswordHash = ComputeSha256Hash(dto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            _context.SaveChanges();

            return Ok("Password has been reset successfully.");
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                var builder = new StringBuilder();
                foreach (var b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }
    }
}
