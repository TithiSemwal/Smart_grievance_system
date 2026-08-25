using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartGrievanceSystem.Core.Helpers;
using SmartGrievanceSystem.Core.Models;
using SmartGrievanceSystem.Infrastructure.Data;
using SmartGrievanceSystem.Web.Models;
using System.Security.Claims;

namespace SmartGrievanceSystem.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email is already in use.");
                    return View(model);
                }

                PasswordHelper.CreatePasswordHash(model.Password, out string hash, out string salt);

                // Defaulting to "User" role for new signups. Assuming RoleID = 1 is User.
                var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");
                if (defaultRole == null)
                {
                    // For scaffolding purposes
                    defaultRole = new Role { RoleName = "User", Description = "Employee" };
                    _context.Roles.Add(defaultRole);
                    await _context.SaveChangesAsync();
                }

                var user = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    RoleID = defaultRole.RoleID,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user == null || !user.IsActive || !PasswordHelper.VerifyPasswordHash(model.Password, user.PasswordHash, user.PasswordSalt))
                {
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
                }

                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.RoleName)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
