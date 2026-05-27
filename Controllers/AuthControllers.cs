using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CampaignApp.Data;
using CampaignApp.Models;

namespace CampaignApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly CampaignContext _context;

        public AuthController(CampaignContext context)
        {
            _context = context;
        }

        // 1. РЕГИСТРАЦИЯ
        [HttpPost("register")]
        public IActionResult Register([FromBody] User registerData)
        {
            if (_context.Users.Any(u => u.Username == registerData.Username))
                return BadRequest("Пользователь с таким логином уже существует!");

            var newUser = new User
        {
            Username = registerData.Username,
            PasswordHash = registerData.PasswordHash,
            Role = "Player",
            FactionId = null, // СТРОГО NULL! Чтобы SQLite не ругалась на FOREIGN KEY
            FactionPointsBalance = 0
        };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok("Регистрация успешна!");
        }

        // 2. ВХОД (LOGIN)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User loginData)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == loginData.Username && u.PasswordHash == loginData.PasswordHash);
            if (user == null) return Unauthorized("Неверный логин или пароль!");

            // Создаем "паспорт" пользователя для кук
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role), // Записываем его роль (Admin/Player)
                new Claim("UserId", user.Id.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return Ok(new { username = user.Username, role = user.Role });
        }

        // 3. ВЫХОД (LOGOUT)
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok("Вышли из аккаунта");
        }

        // 4. ПРОВЕРКА КТО Я (Вызывается фронтендом при загрузке страницы)
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            if (!User.Identity.IsAuthenticated) return Ok(new { role = "Guest" });

            return Ok(new { 
                username = User.Identity.Name, 
                role = User.FindFirst(ClaimTypes.Role)?.Value 
            });
        }
    }
}