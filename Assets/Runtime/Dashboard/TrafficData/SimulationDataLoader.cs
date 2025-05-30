using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrafficSimulationTool.Runtime.SimData;
using TrafficSimulationTool.Runtime.Util;
using Landscape2.Maebashi.Runtime.Util;

namespace Landscape2.Maebashi.Runtime.Dashboard
{
    public class SimulationDataLoader
    {
        private const string CSV_TRAFFIC_RESOURCE_PATH = "traffic_data";
        private const string CSV_VEHICLE_RESOURCE_PATH = "vehicle_data";
        private const string CSV_RESOURCE_DATE_0 = "20240109";
        private const string CSV_RESOURCE_DATE_1 = "20240116";

        private string GetDateString(int dateID)
        {
            switch (dateID)
            {
                case 0:
                    return CSV_RESOURCE_DATE_0;
                case 1:
                    return CSV_RESOURCE_DATE_1;
                default:
                    throw new ArgumentException("不正なdateIDです");
            }
        }

        public List<VehicleTimeline> LoadVehicleData(int dateID)
        {
            var path = CSV_VEHICLE_RESOURCE_PATH + "_" + GetDateString(dateID);
            return CsvDataLoader.LoadCsvData<VehicleTimeline>(
                path,
                true,
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

        public List<RoadIndicator> LoadTrafficData(int dateID)
        {
            var path = CSV_TRAFFIC_RESOURCE_PATH + "_" + GetDateString(dateID);
            return CsvDataLoader.LoadCsvData<RoadIndicator>(
                path,
                true,
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