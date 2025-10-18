using AutoServiceWeb.Data;
using AutoServiceWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AutoServiceWeb.Controllers
{
    public class ClientController : Controller
    {
        ApplicationContext db;
        public ClientController(ApplicationContext context)
        {
            db = context;
        }

        [HttpGet]
        public async Task<IActionResult> Services() => View(await db.Services.ToListAsync());

        [HttpGet]
        public async Task<IActionResult> GetServiceById(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            Service service = await db.Services.FirstOrDefaultAsync(s => s.Id == id);

            if (service == null) return NotFound();

            return View(service);
        }

        [HttpGet]
        public async Task<IActionResult> Profile(string userId)
        {
            var currentUserId = HttpContext.Session.GetString("user");
            if (currentUserId == null)
                return Unauthorized();

            if (string.IsNullOrEmpty(userId)) return NotFound();

            User user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            if (currentUserId != user.Id) return Forbid();

            return View(user);
        }
    }
}
