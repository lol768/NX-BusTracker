using System.Collections.Generic;
using BusTracker.Models;

namespace BusTracker.Events
{
    public class DataChangedEvent
    {
        public IDictionary<string, List<BusLocation>> Data { get; set; }
    }
}