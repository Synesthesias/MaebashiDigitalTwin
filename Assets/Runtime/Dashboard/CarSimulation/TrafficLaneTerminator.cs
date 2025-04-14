using UnityEngine;
using System.Collections.Generic;
using AWSIM.TrafficSimulation;
using Landscape2.Maebashi.Runtime;
using System.Linq;

namespace Landscape2.Maebashi.Runtime
{
    public class TrafficLaneTerminator : MonoBehaviour
    {
        [System.Serializable]
        public struct IntersectionConnection
        {
            [Tooltip("道路名")]
            public string laneName;

            [Tooltip("接続するレーン")]
            public List<TrafficLane> lanes;
        }
        
        [Header("終端設定")]
        [Tooltip("交差点ごとの終端レーン設定")]
        [SerializeField]
        private List<IntersectionConnection> intersectionConnections = new List<IntersectionConnection>();
        

        public void Initialize()
        {
            var allTrafficLanes = GameObject.FindObjectsOfType<TrafficLane>();

            foreach (var lane in allTrafficLanes)
            {
                if (lane == null) continue;
                
                // 交差点名を含むレーンをチェック
                // 交差点から先は進めないように
                bool isIntersectionLane = lane.gameObject.name.Contains("_Intersection_");
                
                // 交差点接続リストに含まれるレーンをチェック
                bool isInConnectionList = intersectionConnections.Any(connection => connection.lanes.Contains(lane));

                if (isIntersectionLane || isInConnectionList)
                {
                    lane.NextLanes.Clear();
                }
            }
        }
    }
} 