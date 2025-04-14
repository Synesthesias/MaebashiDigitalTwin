using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrafficSimulationTool.Runtime.SimData;
using TrafficSimulationTool.Runtime.Util;
using Landscape2.Maebashi.Runtime.Util;

namespace Landscape2.Maebashi.Runtime.Dashboard
{
    public class TrafficDataLoader
    {
        private const string CSV_RESOURCE_PATH = "traffic_data";
        
        public List<RoadIndicator> LoadTrafficData()
        {
            return CsvDataLoader.LoadCsvData<RoadIndicator>(
                CSV_RESOURCE_PATH, // .csvは不要
                true, // ヘッダーをスキップ
                values =>
                {
                    if (values.Length < 5) return null;

                    float trafficVolume = float.Parse(values[3]);
                    float trafficSpeed = TrafficVolumeUtil.CalculateTrafficSpeed(trafficVolume);

                    return new RoadIndicator
                    {
                        StartTime = values[0],
                        EndTime = values[1],
                        LinkID = values[2],
                        TrafficVolume = trafficVolume,
                        TrafficSpeed = trafficSpeed
                    };
                });
        }
    }
} 