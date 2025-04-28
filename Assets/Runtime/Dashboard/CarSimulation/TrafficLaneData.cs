using UnityEngine;
using System.Collections.Generic;
using AWSIM.TrafficSimulation;
using PlateauToolkit.Sandbox.RoadNetwork;
using TrafficSimulationTool.Runtime.SimData;
using System.Linq;
using TrafficSimulationTool.Runtime.Util;
using Landscape2.Maebashi.Runtime.Util;

namespace Landscape2.Maebashi.Runtime
{
    public class TrafficLaneData : MonoBehaviour
    {
        [System.Serializable]
        public class TrafficLaneWithData
        {
            [Tooltip("道路名")]
            public string laneName;
            
            [Tooltip("CSVデータのLinkID")]
            public string linkID;

            [Tooltip("交通量")]
            public float trafficVolume;

            [Tooltip("接続するレーン")]
            public List<TrafficLane> lanes;
            
            [Tooltip("アクティブかどうか")]
            public bool isEnabled = true;
        }

        [Header("車両設定")]
        [Tooltip("スポーンする車両のプレハブ")]
        [SerializeField]
        private GameObject[] vehiclePrefabs;

        [Header("レーン設定")]
        [Tooltip("交通シミュレーション用のレーン")]
        [SerializeField]
        private List<TrafficLaneWithData> trafficLanes = new List<TrafficLaneWithData>();
        
        [Tooltip("次に進むレーンの削除")]
        [SerializeField]
        private TrafficLaneTerminator terminator;

        public GameObject[] VehiclePrefabs => vehiclePrefabs;
        public List<TrafficLaneWithData> TrafficLanes => trafficLanes;

        private bool isInitialized = false;
        
        public void Initialize()
        {
            if (isInitialized) return;
            isInitialized = true;
            terminator.Initialize();
        }

        public void UpdateTrafficVolumes(List<RoadIndicator> indicators, float timeValue)
        {
            if (indicators == null) return;

            foreach (var indicator in indicators)
            {
                // 時間帯に応じた交通量を設定
                string datePart = indicator.StartTime.Substring(0, 8); // yyyyMMdd部分を取得
                int hours = Mathf.FloorToInt(timeValue * 24);
                int minutes = Mathf.FloorToInt((timeValue * 24 - hours) * 60);
                string timeString = $"{datePart}{hours:00}{minutes:00}00";
                double currentTimeSeconds = TimelineUtil.GetTimeSeconds(timeString);
                
                if (currentTimeSeconds >= indicator.StartTimeSeconds && currentTimeSeconds <= indicator.EndTimeSeconds)
                {
                    var lane = trafficLanes.FirstOrDefault(l => l.linkID == indicator.LinkID);
                    if (lane?.lanes.Count > 0)
                    {
                        lane.trafficVolume = indicator.TrafficVolume;
                    }
                }
            }
        }
    }
} 