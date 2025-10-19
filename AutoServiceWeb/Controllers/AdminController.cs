using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AutoServiceWeb.Models;
using Microsoft.EntityFrameworkCore;
using AutoServiceWeb.Data;

namespace AutoServiceWeb.Controllers
{

    public class AdminController : Controller
    {
        ApplicationContext db;
        public AdminController(ApplicationContext context)
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
        public IActionResult CreateService() => View();

        [HttpPost]
        public async Task<IActionResult> CreateService(Service service)
        {
            db.Services.Add(service);
            await db.SaveChangesAsync();

            return RedirectToAction("Services");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateService(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            Service service = await db.Services.FirstOrDefaultAsync(s => s.Id == id);

            if (service == null) return NotFound();

            return View(service);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateService(Service service)
        {
            db.Services.Update(service);
            await db.SaveChangesAsync();

            return RedirectToAction("Services");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteService(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            Service service = await db.Services.FirstOrDefaultAsync(s => s.Id == id);
            if (service == null) return NotFound();

            db.Services.Remove(service);
            await db.SaveChangesAsync();

            return RedirectToAction("Services");
        }

        [HttpGet]
        public async Task<IActionResult> Clients() => View(await db.Users.Where(u => u.Role == "Client").ToListAsync());


        [HttpGet]
        public async Task<IActionResult> GetClientById(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();  

            User user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);

            if(user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            User user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            db.Users.Remove(user);
            await db.SaveChangesAsync();

            return RedirectToAction("Clients");
        }
    }   
}
