using Landscape2.Maebashi.Runtime.Dashboard;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TrafficSimulationTool.Runtime;
using TrafficSimulationTool.Runtime.Simulator;
using TrafficSimulationTool.Runtime.SimData;
using PLATEAU.CityInfo;
using TrafficSimulationTool.Runtime.Util;

namespace Landscape2.Maebashi.Runtime
{
    /// <summary>
    /// 交通シミュレーション全体を管理するクラス
    /// </summary>
    public class TrafficSimulationManager
    {
        public static readonly float PLAYBACK_SPEED = 8f;
        public static readonly uint FPS = 3;

        private readonly GameObject gameObject;
        private readonly SimRoadNetworkManager roadNetworkManager;
        private readonly PLATEAUInstancedCityModel cityModel;
        private DataManager dataManager;
        private TimelineManager timelineManager;

        private VehicleSimulator vehicleSimulator;
        private TrafficSimulator trafficSimulator;

        private TrafficSystemMediatorForHumanFlow humanFlowSystemBridge;
        public TrafficSystemMediatorForHumanFlow HumanFlowSystemBridge => humanFlowSystemBridge;

        private TrafficHeatmapManager heatmapManager;
        public TrafficHeatmapManager HeatmapManager => heatmapManager;

        private TrafficCarSimulationManager carSimulationManager;
        public TrafficCarSimulationManager CarSimulationManager => carSimulationManager;
        
        private Dictionary<int, List<RoadIndicator>> roadDataDictionary = new ();
        private Dictionary<int, List<VehicleTimeline>> vehicleDataDictionary = new ();
        
        // 現在選択中のdateID
        private int currentDateID = 0;

        public TrafficSimulationManager(GameObject gameObject, SimRoadNetworkManager roadNetworkManager, PLATEAUInstancedCityModel cityModel, TrafficSystemMediatorForHumanFlow humanFlowSystemBridge)
        {
            this.gameObject = gameObject ?? throw new ArgumentNullException(nameof(gameObject));
            this.roadNetworkManager = roadNetworkManager ?? throw new ArgumentNullException(nameof(roadNetworkManager));
            this.cityModel = cityModel ?? throw new ArgumentNullException(nameof(cityModel));
            this.humanFlowSystemBridge = humanFlowSystemBridge;            
            _ = Initialize();
        }

        public async Task Initialize()
        {
            InitializeComponents();
            await InitializeData();
        }

        private void InitializeComponents()
        {
            // コンポーネントの追加
            dataManager = gameObject.AddComponent<DataManager>();
            timelineManager = gameObject.AddComponent<TimelineManager>();
            
            // シミュレーターの作成
            vehicleSimulator = new VehicleSimulator();
            trafficSimulator = new TrafficSimulator();
            
            // 交通量ヒートマップの管理クラスの作成
            heatmapManager = new TrafficHeatmapManager();
            carSimulationManager = new TrafficCarSimulationManager();

            // 道路ネットワークの初期化
            roadNetworkManager.Initialize();

            // 各シミュレーターの初期化
            InitializeSimulators();
        }

        private void InitializeSimulators()
        {
            vehicleSimulator.Initialize();
            trafficSimulator.Initialize();
            vehicleSimulator.InitializeReferences(cityModel.GeoReference, roadNetworkManager);
            trafficSimulator.InitializeReferences(cityModel.GeoReference, roadNetworkManager);
            humanFlowSystemBridge.Initialize();
        }

        private async Task InitializeData()
        {
            var sw = new DebugStopwatch();
            DebugLogger.Log(10, "InitializeData start ", "green");

            try
            {
                // CSVデータのロード
                var dataLoader = new SimulationDataLoader();

                // 交通データの読み込み
                for (int dateID = 0; dateID <= 1; dateID++)
                {
                    var trafficData = dataLoader.LoadTrafficData(dateID);
                    if (trafficData != null && trafficData.Any())
                    {
                        roadDataDictionary[dateID] = trafficData;
                    }
                    else
                    {
                        Debug.LogWarning($"交通データの読み込みに失敗しました: dateID={dateID}");
                    }
                }

                // 車両データの読み込み
                for (int dateID = 0; dateID <= 1; dateID++)
                {
                    var vehicleData = dataLoader.LoadVehicleData(dateID);
                    if (vehicleData != null && vehicleData.Any())
                    {
                        vehicleDataDictionary[dateID] = vehicleData;
                    }
                    else
                    {
                        Debug.LogWarning($"車両データの読み込みに失敗しました: dateID={dateID}");
                    }
                }

                // データセットの初期化と登録
                await InitializeDataSets(roadDataDictionary[currentDateID], vehicleDataDictionary[currentDateID]);
            }
            catch (Exception ex)
            {
                Debug.LogError($"データの初期化中にエラーが発生しました: {ex.Message}");
                return;
            }

            DebugLogger.Log(10, $"InitializeData Finish {sw.GetTimeSeconds()}", "green");
        }

        private async Task InitializeDataSets(List<RoadIndicator> trafficData, List<VehicleTimeline> vehicleData)
        {
            try
            {
                // 交通データセットの初期化
                var roadIndicatorDataSet = new RoadIndicatorDataSet();
                roadIndicatorDataSet.Initialize("TrafficHeatmap", new List<object>(trafficData));
                dataManager.AddDataSet(roadIndicatorDataSet);
                
                // 車両データセットの初期化
                var vehicleTimelineDataSet = new VehicleTimelineDataSet();
                vehicleTimelineDataSet.Initialize("VehicleTimeline", new List<object>(vehicleData));
                dataManager.AddDataSet(vehicleTimelineDataSet);

                // 車両シミュレーションにデータを設定
                carSimulationManager.SetTrafficData(trafficData);

                // GeoReferenceを使用してデータセットを初期化
                if (!dataManager.CurrentDataSets.Initialized)
                {
                    var success = await dataManager.CurrentDataSets?.Initialize(FPS, cityModel.GeoReference)!;
                    if (!success)
                    {
                        Debug.LogError("データセットの初期化に失敗しました。");
                        return;
                    }
                }

                // 各シミュレーターにデータを設定
                ConfigureSimulators(roadIndicatorDataSet, vehicleTimelineDataSet);
                
                ConfigureTimeline();
            }
            catch (Exception ex)
            {
                Debug.LogError($"データセットの初期化中にエラーが発生しました: {ex.Message}");
            }
        }

        private void ConfigureSimulators(RoadIndicatorDataSet roadIndicatorDataSet, VehicleTimelineDataSet vehicleTimelineDataSet)
        {
            trafficSimulator.SetData(roadIndicatorDataSet);
            vehicleSimulator.SetData(vehicleTimelineDataSet);
        }

        private void ConfigureTimeline()
        {
            timelineManager.SetDurationData(dataManager.CurrentDataSets?.duration);
            timelineManager.SetSpeed(PLAYBACK_SPEED);
            timelineManager.AddSequence(trafficSimulator);
        }

        public void UpdateBasedOnTime(float newValue)
        {
            if (heatmapManager.IsHeatmapEnabled)
            {
                UpdateTimeline(newValue);
            }

            carSimulationManager.UpdateTrafficDataForCurrentTime(newValue);

            humanFlowSystemBridge?.SetTime(newValue);
        }
        
        public void UpdateTimeline(float newValue)
        {
            timelineManager?.Move(newValue);
        }

        public void UpdateDate(int dateID, float timeValue)
        {
            humanFlowSystemBridge?.SetDateId(dateID);
            _ = SwitchDataByDateID(dateID, timeValue);
        }

        private async Task SwitchDataByDateID(int dateID, float timeValue)
        {
            if (!roadDataDictionary.ContainsKey(dateID) || !vehicleDataDictionary.ContainsKey(dateID))
            {
                Debug.LogWarning($"指定されたdateIDのデータが存在しません: dateID={dateID}");
                return;
            }
            currentDateID = dateID;
            // データセットの再初期化
            await InitializeDataSets(roadDataDictionary[dateID], vehicleDataDictionary[dateID]);

            UpdateBasedOnTime(timeValue);
        }
    }
} 