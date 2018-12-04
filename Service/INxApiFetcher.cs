using System.Collections.Generic;
using BusTracker.Models;

namespace BusTracker.Service
{
    public interface INxApiFetcher
    {
        IDictionary<string, List<BusLocation>> GetData();
    }
}