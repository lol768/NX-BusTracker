using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BusTracker.Models;
using BusTracker.Service;
using CoordinateSharp;

namespace BusTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly INxApiFetcher _api;
        private Coordinate _warwickCampus;

        public HomeController(INxApiFetcher api)
        {
            _api = api;
        }

        public IActionResult Index(string forceTheme)
        {
            var dateTime = DateTime.UtcNow;
            _warwickCampus = new Coordinate(52.381510, -1.561725, dateTime);
            ViewData["theme"] = "dark";
            Console.WriteLine(_warwickCampus.CelestialInfo.SunSet);
            if (_warwickCampus.CelestialInfo.SunSet.GetValueOrDefault(DateTime.UtcNow.Date.AddHours(17)).Subtract(dateTime).Ticks > 0)
            {
                ViewData["theme"] = "light";
            }

            if (HasValidTheme(forceTheme))
            {
                ViewData["theme"] = forceTheme;
            }
            return View();
        }

        private bool HasValidTheme(string theme)
        {
            if (theme == "light" || theme == "dark") return true;
            return false;
        }

        [ResponseCache(Duration = 30)]
        public IActionResult Data()
        {
            return Json(_api.GetData());
        }
      
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}