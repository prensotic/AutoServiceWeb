using AutoServiceWeb.Data;
using AutoServiceWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace AutoServiceWeb.Controllers
{
    public class AuthController : Controller
    {
        ApplicationContext db;
        public AuthController(ApplicationContext context)
        {
            db = context;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (user == null) return BadRequest();

            var passwordHasher = new PasswordHasher<User>();

            user.Id = Guid.NewGuid().ToString();
            user.Password = passwordHasher.HashPassword(user, user.Password);
            user.Role = "Client";

            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string password, string email)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return Unauthorized();

            var passwordHasher = new PasswordHasher<User>();
            var result = passwordHasher.VerifyHashedPassword(user, user.Password, password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized();

            HttpContext.Session.SetString("userId", user.Id);
            HttpContext.Session.SetString("userRole", user.Role);
            HttpContext.Session.SetString("userName", user.FullName);

            return RedirectToAction("Profile", "Client", new { userId = user.Id });
        }
    }
}
