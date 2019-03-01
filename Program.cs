using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BusTracker.Models;
using BusTracker.Service;
using Itinero;
using Itinero.IO.Osm;
using Itinero.LocalGeo;
using Itinero.Profiles;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vehicle = Itinero.Osm.Vehicles.Vehicle;

namespace BusTracker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
            
//            RouterDb routerDb;
//            var vehicle = DynamicVehicle.LoadFromStream(File.OpenRead("bus_my.lua"));
//
//            // read the routerdb from disk.
//            using (var stream = new FileInfo(@"england-latest-bus.db").Open(FileMode.Open))
//            {
//                routerDb = RouterDb.Deserialize(stream);
//            }
//
//
//            var router = new Router(routerDb);
//            var coordinates = GetCoords12XInbound();
//            // calculate a route.
//            var route = router.Calculate(vehicle.Fastest()
//                , coordinates);
//            var geoJson = route.ToGeoJson();
//            Console.WriteLine(geoJson);
        }

        public static Coordinate[] GetCoords12XInbound()
        {
            return new Coordinate[]
            {
                new Coordinate(52.37945f, -1.56263f),
                new Coordinate(52.37425f, -1.55391f),
                new Coordinate(52.37111f, -1.54986f),
                new Coordinate(52.37677f, -1.54446f),
                new Coordinate(52.38285f, -1.53849f),
                new Coordinate(52.38716f, -1.53436f),
                new Coordinate(52.39036f, -1.53125f),
                new Coordinate(52.39311f, -1.52862f),
                new Coordinate(52.39452f, -1.52405f),
                new Coordinate(52.39566f, -1.51894f),
                new Coordinate(52.39737f, -1.51686f),
                new Coordinate(52.40057f, -1.51612f),
                new Coordinate(52.40178f, -1.51427f),
                new Coordinate(52.4056f, -1.51164f),
                new Coordinate(52.40659f, -1.50859f),
                new Coordinate(52.40692f, -1.50675f),
                new Coordinate(52.408f, -1.50378f),
                new Coordinate(52.40943f, -1.50513f),
                new Coordinate(52.41067f, -1.5075f)
            };
        }

        public static Coordinate[] GetCoords11Outbound()
        {
            return new Coordinate[]
            {
                new Coordinate(52.41067f, -1.5075f),
                new Coordinate(52.40849f, -1.50347f),
                new Coordinate(52.40721f, -1.50383f),
                new Coordinate(52.40662f, -1.50596f),
                new Coordinate(52.40529f, -1.50822f),
                
                new Coordinate(52.40487f,-1.50987f), // fake
                
                new Coordinate(52.40545f, -1.51172f),
                new Coordinate(52.40196f, -1.51418f),
                new Coordinate(52.40107f, -1.51557f),
                new Coordinate(52.3988f, -1.51758f),
                new Coordinate(52.3994f, -1.52072f),
                new Coordinate(52.39831f, -1.52282f),
                new Coordinate(52.39749f, -1.52414f),
                new Coordinate(52.39698f, -1.52738f),
                new Coordinate(52.39953f, -1.53017f),
                new Coordinate(52.40242f, -1.53273f),
                new Coordinate(52.40464f, -1.5357f),
                new Coordinate(52.40344f, -1.54155f),
                new Coordinate(52.4037f, -1.54743f),
                new Coordinate(52.40264f, -1.55126f),
                new Coordinate(52.40254f, -1.5552f),
                new Coordinate(52.39139f, -1.55314f),
                new Coordinate(52.38952f, -1.55696f),
                new Coordinate(52.38746f, -1.56237f),
                new Coordinate(52.3853f, -1.5666f),
                new Coordinate(52.38438f, -1.56722f),
                new Coordinate(52.3827f, -1.56495f),
                new Coordinate(52.37962f, -1.56252f),
                new Coordinate(52.37425f, -1.55391f),
                new Coordinate(52.3705f, -1.55013f),
                new Coordinate(52.36775f, -1.55276f),
                new Coordinate(52.35867f, -1.56532f),
                new Coordinate(52.35692f, -1.56768f),
                new Coordinate(52.35344f, -1.576f),
                new Coordinate(52.35125f, -1.57804f),
                new Coordinate(52.34825f, -1.57916f),
                new Coordinate(52.34627f, -1.57816f),
                new Coordinate(52.34387f, -1.57993f),
                new Coordinate(52.34155f, -1.57789f),
                new Coordinate(52.33883f, -1.57502f),
                new Coordinate(52.33702f, -1.57314f),
                new Coordinate(52.33566f, -1.57081f),
                new Coordinate(52.32713f, -1.55836f),
                new Coordinate(52.32532f, -1.55465f),
                new Coordinate(52.31946f, -1.54419f),
                new Coordinate(52.31309f, -1.5397f),
                new Coordinate(52.3029f, -1.53696f),
                new Coordinate(52.30038f, -1.53622f),
                new Coordinate(52.29729f, -1.53546f),
                new Coordinate(52.29249f, -1.53574f),
                new Coordinate(52.2898f, -1.5353f),
                new Coordinate(52.28807f, -1.53476f),
                new Coordinate(52.28597f, -1.53301f),
                new Coordinate(52.2848f, -1.5326f), 
                new Coordinate(52.28397f, -1.53298f)
            };
        }

        private static Coordinate[] GetCoords11Inbound()
        {
            return new Coordinate[]
            {
                new Coordinate(52.28397f, -1.53298f),
                new Coordinate(52.2856f, -1.5347f),
                new Coordinate(52.28624f, -1.53333f),
                new Coordinate(52.28822f, -1.53514f),
                new Coordinate(52.29005f, -1.53565f),
                new Coordinate(52.2926f, -1.5361f),
                new Coordinate(52.29438f, -1.5354f),
                new Coordinate(52.29674f, -1.53567f),
                new Coordinate(52.29881f, -1.53606f),
                new Coordinate(52.30113f, -1.5367f),
                new Coordinate(52.30312f, -1.53728f),
                new Coordinate(52.30854f, -1.53875f),
                new Coordinate(52.31162f, -1.53952f),
                new Coordinate(52.31964f, -1.54482f),
                new Coordinate(52.32525f, -1.55493f),
                new Coordinate(52.32875f, -1.5604f),
                new Coordinate(52.33236f, -1.56534f),
                new Coordinate(52.33544f, -1.57097f),
                new Coordinate(52.33794f, -1.57438f),
                new Coordinate(52.3411f, -1.57767f),
                new Coordinate(52.34411f, -1.58062f),
                new Coordinate(52.34626f, -1.57855f),
                new Coordinate(52.34882f, -1.58005f),
                new Coordinate(52.35107f, -1.57898f),
                new Coordinate(52.35355f, -1.57626f),
                new Coordinate(52.35748f, -1.56725f),
                new Coordinate(52.35856f, -1.5658f),
                new Coordinate(52.36114f, -1.56259f),
                new Coordinate(52.368f, -1.55267f),
                new Coordinate(52.37464f, -1.55585f),
                new Coordinate(52.37972f, -1.56294f),
                new Coordinate(52.3832f, -1.56575f),
                new Coordinate(52.38447f, -1.56747f),
                new Coordinate(52.38786f, -1.56169f),
                new Coordinate(52.38974f, -1.55675f),
                new Coordinate(52.39174f, -1.55287f),
                new Coordinate(52.39968f, -1.55837f),
                new Coordinate(52.40276f, -1.55552f),
                new Coordinate(52.40275f, -1.5512f),
                new Coordinate(52.40377f, -1.54778f),
                new Coordinate(52.40354f, -1.54261f),
                new Coordinate(52.40451f, -1.53491f),
                new Coordinate(52.40233f, -1.53211f),
                new Coordinate(52.40025f, -1.53052f),
                new Coordinate(52.3966f, -1.52623f),
                new Coordinate(52.39748f, -1.52437f),
                new Coordinate(52.39864f, -1.52262f),
                new Coordinate(52.39958f, -1.52013f),
                new Coordinate(52.39906f, -1.5182f),
                new Coordinate(52.40057f, -1.51612f),
                new Coordinate(52.40178f, -1.51427f),
                new Coordinate(52.4056f, -1.51164f),
                new Coordinate(52.40659f, -1.50859f),
                new Coordinate(52.40692f, -1.50675f),
                new Coordinate(52.408f, -1.50378f),
                new Coordinate(52.40943f, -1.50513f),
                new Coordinate(52.41067f, -1.5075f)
            };
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}