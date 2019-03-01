using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BusTracker.Events;
using Microsoft.AspNetCore.Mvc;
using BusTracker.Models;
using BusTracker.Service;
using CoordinateSharp;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace BusTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly INxApiFetcher _api;
        private Coordinate _warwickCampus;
        private JsonSerializerSettings _opts;

        public HomeController(INxApiFetcher api, IOptions<MvcJsonOptions> opts)
        {
            this._opts = opts.Value.SerializerSettings;
            _api = api;
        }

        public IActionResult Index(string forceTheme)
        {
            var dateTime = DateTime.UtcNow;
            _warwickCampus = new Coordinate(52.381510, -1.561725, dateTime);
            ViewData["theme"] = "dark";
            Console.WriteLine(_warwickCampus.CelestialInfo.SunSet);
            if (_warwickCampus.CelestialInfo.SunSet.GetValueOrDefault(DateTime.UtcNow.Date.AddHours(17))
                    .Subtract(dateTime).Ticks > 0)
            {
                ViewData["theme"] = "light";
            }

            if (HasValidTheme(forceTheme))
            {
                ViewData["theme"] = forceTheme;
            }

            return View();
        }

        public async Task<IActionResult> Websocket()
        {
            var context = HttpContext;
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                await BusData(context, webSocket);
                return Ok("WebSocket");
            }

            return BadRequest("Must upgrade to WS");
        }

        private async Task BusData(HttpContext context, WebSocket webSocket)
        {
            Action<DataChangedEvent> listener = async e =>
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    await webSocket.SendAsync(
                        new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(e.Data, _opts))),
                        WebSocketMessageType.Text, true, CancellationToken.None);
                }
            };
            
            var buffer = new byte[1024 * 4];
            this._api.Changed += listener;

            WebSocketReceiveResult result =
                await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                Console.WriteLine("Entering while loop!");
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            Console.WriteLine("Closing websocket");
            this._api.Changed -= listener;
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
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