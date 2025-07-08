using UnityEngine;
using AWSIM.TrafficSimulation;
using System.Linq;
using TrafficSimulationTool.Runtime.SimData;
using System.Collections.Generic;
using Landscape2.Maebashi.Runtime.Util;

namespace Landscape2.Maebashi.Runtime
{
    /// <summary>
    /// 交通シミュレーションの管理クラス
    /// </summary>
    public class TrafficCarSimulationManager
    {
        private TrafficLaneData spawnLaneData;
        private TrafficManager trafficManager;
        private List<RoadIndicator> currentTrafficData;
        private bool isCarSimulationEnabled;

        public bool IsCarSimulationEnabled => isCarSimulationEnabled;

        public TrafficCarSimulationManager()
        {
            InitializeComponents();
        }

        /// <summary>
        /// 必要なコンポーネントの初期化
        /// </summary>
        private void InitializeComponents()
        {
            trafficManager = Object.FindObjectOfType<TrafficManager>();
            spawnLaneData = Object.FindObjectOfType<TrafficLaneData>();

            if (trafficManager == null)
            {
                Debug.LogError("TrafficManagerコンポーネントが見つかりません。");
                return;
            }

            if (spawnLaneData == null)
            {
                Debug.LogError("SpawnLaneDataコンポーネントが見つかりません。");
                return;
            }

            // 初期状態は非表示
            trafficManager.Initialize();
            trafficManager.gameObject.SetActive(false);
        }

        /// <summary>
        /// 交通データを設定
        /// </summary>
        public void SetTrafficData(List<RoadIndicator> trafficData)
        {
            currentTrafficData = trafficData;
        }

        /// <summary>
        /// 現在時刻の交通データを更新
        /// </summary>
        public void UpdateTrafficDataForCurrentTime(float timeValue)
        {
            if (currentTrafficData == null) return;

            spawnLaneData.UpdateTrafficVolumes(currentTrafficData, timeValue);
            if (isCarSimulationEnabled)
            {
                UpdateTrafficRoutes();
            }
        }

        /// <summary>
        /// 車両シミュレーションの有効/無効を設定
        /// </summary>
        public void SetCarSimulationEnabled(bool enabled)
        {
            isCarSimulationEnabled = enabled;
            if (trafficManager != null)
            {
                trafficManager.gameObject.SetActive(enabled);
            }
            
            if (enabled)
            {
                spawnLaneData.Initialize();
                UpdateTrafficRoutes();
            }
        }

        /// <summary>
        /// 交通ルートの更新
        /// </summary>
        private void UpdateTrafficRoutes()
        {
            if (spawnLaneData == null) return;

            // レーンごとの設定を適用
            var spawnSettings = new List<RouteTrafficSimulatorConfiguration>();
            foreach (var trafficLane in spawnLaneData.TrafficLanes)
            {
                if (trafficLane.trafficVolume <= 0)
                {
                    continue;
                }
                var spawnSetting = new RouteTrafficSimulatorConfiguration
                {
                    npcPrefabs = spawnLaneData.VehiclePrefabs,
                    route = trafficLane.lanes.ToArray(), // ダミーのルート（spawnableLanes使用時は無視される）
                    spawnableLanes = trafficLane.lanes.ToArray(), // 全車線でランダムスポーン
                    maximumSpawns = 0, // 無限にスポーン
                    enabled = trafficLane.isEnabled,
                    spawnIntervalTime = TrafficVolumeUtil.CalculateSpawnInterval(trafficLane.trafficVolume),
                };
                spawnSettings.Add(spawnSetting);
            }

            trafficManager.routeTrafficSims = spawnSettings.ToArray();
            trafficManager.Restart(0, 300);
        }
    }
}