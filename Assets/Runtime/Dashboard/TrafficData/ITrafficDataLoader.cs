using System.Collections.Generic;
using System.Threading.Tasks;
using TrafficSimulationTool.Runtime.SimData;

namespace Landscape2.Maebashi.Runtime.Dashboard
{
    public interface ITrafficDataLoader
    {
        Task<List<TrafficData>> LoadTrafficData();
    }
} 