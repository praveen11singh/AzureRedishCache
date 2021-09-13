using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using RedishCache.DB;
using RedishCache.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace RedishCache.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IDistributedCache redisCache;
        private AppDbContext db;
        public HomeController(AppDbContext db, IDistributedCache cache, ILogger<HomeController> logger)
        {
            this.redisCache = cache;
            this.db = db;
            _logger = logger;
        }   

        public IActionResult Index()
        {
            string jsonEmployees = redisCache.GetString("employees");

            if (jsonEmployees == null)
            {
                List<Employee> employees = db.Employee.Take(9).ToList();
                jsonEmployees = JsonSerializer.Serialize<List<Employee>>(employees);
                var options = new  DistributedCacheEntryOptions();
                options.SetAbsoluteExpiration (DateTimeOffset.Now.AddMinutes(1));
                redisCache.SetString("employees",jsonEmployees, options);
            }

            JsonSerializerOptions opt = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };
            List<Employee> data = JsonSerializer.Deserialize<List<Employee>>(jsonEmployees, opt);
            return View(data);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
