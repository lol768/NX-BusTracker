using System;
using System.Collections.Generic;
using System.Threading;
using BusTracker.Events;
using BusTracker.Models;

namespace BusTracker.Service
{
    public interface INxApiFetcher
    {
        IDictionary<string, List<BusLocation>> GetData();

        event Action<DataChangedEvent> Changed;
        
    }
}