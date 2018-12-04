using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BusTracker.Models;
using BusTracker.Service;

namespace BusTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly INxApiFetcher _api;

        public HomeController(INxApiFetcher api)
        {
            _api = api;
        }

        public IActionResult Index()
        {
            return View();
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