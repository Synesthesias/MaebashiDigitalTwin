using System.Collections.Generic;
using System.Threading.Tasks;
using TrafficSimulationTool.Runtime.SimData;
using TrafficSimulationTool.Runtime.Util;

namespace Landscape2.Maebashi.Runtime.Dashboard
{
    public class VehicleDataLoader
    {
        private const string CSV_RESOURCE_PATH = "vehicle_data";
        
        public async Task<List<VehicleTimeline>> LoadVehicleData()
        {
            return await CsvDataLoader.LoadCsvData<VehicleTimeline>(
                CSV_RESOURCE_PATH, // .csvは不要
                true, // ヘッダーをスキップ
                values =>
                {
                    if (values.Length < 12) return null;

                    return new VehicleTimeline
                    {
                        TimeStamp = values[0],
                        VehicleID = values[1],
                        VehicleType = values[2],
                        Latitude = float.Parse(values[3]),
                        Longitude = float.Parse(values[4]),
                        LinkID = values[5],
                        Offset = float.Parse(values[6]),
                        Lane = float.Parse(values[7]),
                        Track = float.Parse(values[8]),
                        Speed = float.Parse(values[9]),
                        Departure = values[10],
                        Destination = values[11]
                    };
                });
        }
    }
} 