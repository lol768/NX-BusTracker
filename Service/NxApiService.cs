using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using BusTracker.Models;
using CoordinateSharp;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace BusTracker.Service
{
    public class NxApiService : BackgroundService, INxApiFetcher
    {
        private HttpClient httpClient;
        private const string ApiUrl = "https://apinxbus.azure-api.net";

        private ConcurrentDictionary<string, List<BusLocation>> _data =
            new ConcurrentDictionary<string, List<BusLocation>>();

        private static readonly string[] routes = {"12X", "11", "11U", "18A", "18"};

        public NxApiService()
        {
            this.httpClient = new HttpClient();
            this.httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "8bba464bd0e94f2cb544c1486aff48c0");
            this.httpClient.DefaultRequestHeaders.Add("User-Agent", "okhttp/3.4.2");
        }

        public async Task<List<BusLocation>> GetLocationsForRoute(string routeCode, TravelDirection dir,
            CancellationToken token, List<BusLocation> currentData)
        {
            var fullUrl = ApiUrl + $"/routes/api/1/nxb/routes/{HttpUtility.UrlPathEncode(routeCode)}/buslocations";
            var queryParams = new Dictionary<string, string>();
            queryParams["operatorcode"] = "TCV";
            queryParams["direction"] = dir.ToString().ToLowerInvariant();

            fullUrl = QueryHelpers.AddQueryString(fullUrl, queryParams);

            var uuid = System.Guid.NewGuid().ToString();
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(fullUrl),
                Headers =
                {
                    {"X-Device-Id", uuid}
                }
            };
            using (var response = await httpClient.SendAsync(httpRequestMessage))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Bus API isn't happy ({response.StatusCode.ToString()}): " +
                                                        await response.Content.ReadAsStringAsync());
                }

                var decoded =
                    JsonConvert.DeserializeObject<List<BusLocation>>(await response.Content.ReadAsStringAsync());
                foreach (var busLocation in decoded)
                {
                    if (busLocation.BestGuessPastLocations == null)
                    {
                        busLocation.BestGuessPastLocations = new List<BusLocation>();
                    }
                    var locationNow = new Coordinate(busLocation.Latitude, busLocation.Longitude, DateTime.UtcNow);
                    bool Pred(BusLocation di) => new Distance(locationNow, (new Coordinate(di.Latitude, di.Longitude, di.CollectionTimestamp)), Shape.Ellipsoid).Meters < 1;
                    if (!currentData.Exists(Pred))
                    {
                        busLocation.CollectionTimestamp = DateTime.UtcNow; // we can be happy this is a new item
                        (double, BusLocation) Distance(BusLocation di) => (new Distance(locationNow, (new Coordinate(di.Latitude, di.Longitude, di.CollectionTimestamp)), Shape.Ellipsoid).Meters, di);
                        var probableLastBusLocation = currentData.Select(Distance).OrderBy(t => t.Item1);
                        if (!probableLastBusLocation.Any())
                        {
                            continue;
                        }
                        // assume drivers won't speed at too much more than 80mph
                        // 750m is doable in 21 seconds
                        // we poll every 20s ish
                        if (probableLastBusLocation.First().Item1 < 750)
                        {
                            var pastLoc = probableLastBusLocation.First().Item2;
                            busLocation.BestGuessPastLocations.Add(pastLoc);
                            busLocation.BestGuessPastLocations.AddRange(pastLoc.BestGuessPastLocations);
                            pastLoc.BestGuessPastLocations.Clear();
                        }
                        // if it's more than 750m away.. assume either the GPS position was fucked or it was another bus
                    }
                    else
                    {
                        var match = currentData.First(Pred);
                        busLocation.CollectionTimestamp = match.CollectionTimestamp; // this is old, NX haven't updated data
                    }

                    if (busLocation.BestGuessPastLocations.Count == 20)
                    {
                        busLocation.BestGuessPastLocations.RemoveAt(0);
                    }
                }
                
                foreach (var busLocation in decoded)
                {
                    var enoughData = busLocation.BestGuessPastLocations.Count >= 1;
                    if (!enoughData)
                    {
                        busLocation.SpeedGuess = -1;
                        continue;
                    }

                    var totalMph = 0.0;
                    var count = 0;
                    var listCopy =
                        new List<BusLocation>(busLocation.BestGuessPastLocations.OrderBy(bl => bl.CollectionTimestamp))
                        {
                            busLocation
                        };

                    for (var i = 1; i < listCopy.Count; i++)
                    {
                        var l1 = listCopy[i - 1];
                        var l2 = listCopy[i];
                        var loc1 = new Coordinate(l1.Latitude, l1.Longitude, l1.CollectionTimestamp);
                        var loc2 = new Coordinate(l2.Latitude, l2.Longitude, l2.CollectionTimestamp);
                        var dist = new Distance(loc1, loc2, Shape.Ellipsoid);
                        var distMiles = dist.Miles;
                        var timeDelta = l2.CollectionTimestamp - l1.CollectionTimestamp;
                        totalMph += distMiles / timeDelta.TotalHours;
                        count++;
                    }

                    busLocation.SpeedGuess = totalMph / count;
                }
                return decoded;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var route in routes)
                {
                    Console.WriteLine("Fetching for " + route);
                    try
                    {
                        var dataInbound = await GetLocationsForRoute(route, TravelDirection.Inbound, stoppingToken, _data.GetValueOrDefault(route + "-Inbound") ?? new List<BusLocation>());
                        var dataOutbound = await GetLocationsForRoute(route, TravelDirection.Outbound, stoppingToken, _data.GetValueOrDefault(route + "-Outbound") ?? new List<BusLocation>());
                        _data[route + "-Inbound"] = dataInbound;
                        _data[route + "-Outbound"] = dataOutbound;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                        Console.WriteLine("Skipping!");
                    }
                    
                    await Task.Delay(500, stoppingToken);

                }

                await Task.Delay(20000, stoppingToken);
            }
        }

        public IDictionary<string, List<BusLocation>> GetData()
        {
            return _data;
        }
    }
}