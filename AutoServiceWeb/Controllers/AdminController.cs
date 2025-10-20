using AutoServiceWeb.Data;
using AutoServiceWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<IActionResult> Services(
    string search = "",
    decimal? minPrice = null,
    decimal? maxPrice = null,
    int? minDiscount = null,
    int? maxDiscount = null)
        {
            var servicesQuery = db.Services.Include(s => s.AdditionalImages).AsQueryable();

            if (!string.IsNullOrEmpty(search))
                servicesQuery = servicesQuery.Where(s => s.Title.Contains(search));

            if (minDiscount.HasValue)
                servicesQuery = servicesQuery.Where(s => s.Discount >= minDiscount.Value);

            if (maxDiscount.HasValue)
                servicesQuery = servicesQuery.Where(s => s.Discount <= maxDiscount.Value);

            // Фильтр по цене с учетом скидки
            if (minPrice.HasValue)
                servicesQuery = servicesQuery.Where(s => (s.Price - s.Price * (s.Discount / 100m)) >= minPrice.Value);

            if (maxPrice.HasValue)
                servicesQuery = servicesQuery.Where(s => (s.Price - s.Price * (s.Discount / 100m)) <= maxPrice.Value);

            var services = await servicesQuery.ToListAsync();

            ViewBag.SearchQuery = search;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.MinDiscount = minDiscount;
            ViewBag.MaxDiscount = maxDiscount;

            return View(services);
        }



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
        public async Task<IActionResult> CreateService(Service service, IFormFile? MainImage)
        {
            if (service == null) return NotFound();

            service.Id = Guid.NewGuid().ToString();

            // Сохраняем файл, если выбран
            if (MainImage != null && MainImage.Length > 0)
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "services");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(MainImage.FileName)}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await MainImage.CopyToAsync(stream);
                }

                service.MainImage = $"/uploads/services/{fileName}";
            }

            db.Services.Add(service);
            await db.SaveChangesAsync();

            return RedirectToAction("Services");
        }


        [HttpGet]
        public async Task<IActionResult> UpdateService(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var service = await db.Services
                .Include(s => s.AdditionalImages)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null) return NotFound();

            return View(service);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateService(Service service, IFormFile? MainImageFile, List<IFormFile> additionalImages)
        {
            var existingService = await db.Services
                .Include(s => s.AdditionalImages)
                .FirstOrDefaultAsync(s => s.Id == service.Id);

            if (existingService == null) return NotFound();

            existingService.Title = service.Title;
            existingService.Price = service.Price;
            existingService.Discount = service.Discount;
            existingService.Time = service.Time;

            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            // Главное изображение
            if (MainImageFile != null && MainImageFile.Length > 0)
            {
                var mainFileName = Guid.NewGuid() + Path.GetExtension(MainImageFile.FileName);
                var mainFilePath = Path.Combine(uploadPath, mainFileName);

                using (var stream = new FileStream(mainFilePath, FileMode.Create))
                {
                    await MainImageFile.CopyToAsync(stream);
                }

                existingService.MainImage = "/uploads/" + mainFileName;
            }

            // Дополнительные изображения
            foreach (var image in additionalImages)
            {
                if (image.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    existingService.AdditionalImages.Add(new ServiceImage
                    {
                        ServiceId = existingService.Id,
                        ImagePath = "/uploads/" + fileName
                    });
                }
            }

            db.Services.Update(existingService);
            await db.SaveChangesAsync();

            return RedirectToAction("Services");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteServiceImage(int id)
        {
            var image = await db.ServiceImages.FindAsync(id);
            if (image == null) return NotFound();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.ImagePath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            db.ServiceImages.Remove(image);
            await db.SaveChangesAsync();

            return RedirectToAction("UpdateService", new { id = image.ServiceId });
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
        public async Task<IActionResult> Clients(string search = "")
        {
            var clientsQuery = db.Users.Where(u => u.Role == "Client").AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                clientsQuery = clientsQuery.Where(c => c.FullName.Contains(search) || c.Email.Contains(search));
            }

            var clients = await clientsQuery.ToListAsync();

            ViewBag.SearchQuery = search;
            return View(clients);
        }


        [HttpGet]
        public async Task<IActionResult> GetClientById(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            User user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteClient(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            User user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            db.Users.Remove(user);
            await db.SaveChangesAsync();

            return RedirectToAction("Clients");
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrder() => View((await db.Users.ToListAsync(), await db.Services.ToListAsync()));

        [HttpPost]
        public async Task<IActionResult> CreateOrder(Order order)
        {
            if (order == null) return NotFound();

            order.Id = Guid.NewGuid().ToString();
            order.CreationDateAndTime = DateTime.Now;
            order.isActive = true;

            await db.Orders.AddAsync(order);
            await db.SaveChangesAsync();
            return RedirectToAction("Orders");
        }

        [HttpGet]
        public async Task<IActionResult> Orders(string filter = "all")
        {
            var ordersQuery = db.Orders
                .Include(o => o.Service)
                .Include(o => o.Client)
                .AsQueryable();

            ordersQuery = ordersQuery.OrderBy(o => o.ServiceDateTime);

            if (filter == "nearest")
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1).AddHours(23).AddMinutes(59);
                ordersQuery = ordersQuery.Where(o => o.ServiceDateTime >= DateTime.Now && o.ServiceDateTime <= tomorrow);
            }

            var orders = await ordersQuery.ToListAsync();
            ViewBag.Filter = filter;
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOrder(string id)
        {
            if (id == null) return NotFound();

            Order order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            db.Orders.Remove(order);
            await db.SaveChangesAsync();
            return RedirectToAction("Orders");
        }

        [HttpPost]
        public async Task<IActionResult> CompleteOrder(string id)
        {
            if (id == null) return NotFound();

            Order order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            order.isActive = false;
            db.Orders.Update(order);
            await db.SaveChangesAsync();

            return RedirectToAction("Orders");
        }
    }
}
