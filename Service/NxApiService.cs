using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using BusTracker.Models;
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
            CancellationToken token)
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
            using (var response = await httpClient.SendAsync(httpRequestMessage, token))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Bus API isn't happy ({response.StatusCode.ToString()}): " +
                                                        await response.Content.ReadAsStringAsync());
                }

                var decoded =
                    JsonConvert.DeserializeObject<List<BusLocation>>(await response.Content.ReadAsStringAsync());
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
                    var dataInbound = await GetLocationsForRoute(route, TravelDirection.Inbound, stoppingToken);
                    var dataOutbound = await GetLocationsForRoute(route, TravelDirection.Outbound, stoppingToken);
                    _data[route + "-Inbound"] = dataInbound;
                    _data[route + "-Outbound"] = dataOutbound;
                    await Task.Delay(500, stoppingToken);

                }

                await Task.Delay(30000, stoppingToken);
            }
        }

        public IDictionary<string, List<BusLocation>> GetData()
        {
            return _data;
        }
    }
}