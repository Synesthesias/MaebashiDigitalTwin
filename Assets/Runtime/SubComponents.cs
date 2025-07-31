using Landscape2.Maebashi.Runtime.Common;
using Landscape2.Maebashi.Runtime.Dashboard;
using Landscape2.Maebashi.Runtime.UICommon.Components;
using Landscape2.Runtime;
using Landscape2.Runtime.BuildingEditor;
using Landscape2.Runtime.CameraPositionMemory;
using Landscape2.Runtime.GisDataLoader;
using Landscape2.Runtime.LandscapePlanLoader;
using Landscape2.Runtime.WalkerMode;
using Landscape2.Runtime.WeatherTimeEditor;
using PLATEAU.CityInfo;
using System.Collections.Generic;
using TrafficSimulationTool.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using ISubComponent = Landscape2.Runtime.ISubComponent;

namespace Landscape2.Maebashi.Runtime
{
    /// <summary>
    /// Main処理
    /// </summary>
    public class SubComponents : MonoBehaviour
    {
        public enum SubMenuUxmlType
        {
            Menu = -1,
            EditBuilding,
            Asset,
            Bim,
            Gis,
            Planning,
            Analytics,
            CameraList,
            CameraEdit,
            WalkMode,
            DashBoard,
        }

        private List<ISubComponent> subComponents = new();
        private SimRoadNetworkManager roadNetworkManager;
        private PLATEAUInstancedCityModel cityModel;

        private TrafficSystemMediatorForHumanFlow humanFlowSystemBridge;

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Awake()
        {
            cityModel = GameObject.FindObjectOfType<PLATEAUInstancedCityModel>();
            if (cityModel == null)
            {
                Debug.LogError("cityModelInstance is Null!");
                return;
            }

            roadNetworkManager = GameObject.FindObjectOfType<SimRoadNetworkManager>();
            if (roadNetworkManager == null)
            {
                Debug.LogError("SimRoadNetworkManager is Null!");
                return;
            }

            humanFlowSystemBridge = GameObject.FindObjectOfType<TrafficSystemMediatorForHumanFlow>();

            LoadCityScene();
            InitializeUIComponents();
        }

        private void LoadCityScene()
        {
            // 分割した街用のシーンをロード
            SceneManager.LoadScene("Maebashi_City_Buildings", LoadSceneMode.Additive);
            SceneManager.LoadScene("Maebashi_City_Road", LoadSceneMode.Additive);
        }

        /// <summary>
        /// UI関連コンポーネントの初期化
        /// </summary>
        private void InitializeUIComponents()
        {
            // 各コンポーネント初期化
            var cameraManager = new CameraManager();

            var uxmlHandler = new UxmlHandler();
            var globalNaviUI = new GlobalNaviUI(uxmlHandler, cameraManager.LandscapeCamera);
            var footerNaviUI = new FooterNaviUI(uxmlHandler, globalNaviUI, cameraManager.LandscapeCamera);

            // 建物表示位置調整
            uxmlHandler.AdjustMargin(SubMenuUxmlType.Asset, "EditBuildingArea", new UxmlStyleMargin()
            {
                top = 0,
                left = 0,
                right = 0,
                bottom = 150, // 視点操作UIとかぶるので、位置を少し上にずらす
            });
            
            var cameraAutoRotate = new CameraAutoRotate();
            var saveSystem = new SaveSystem(globalNaviUI.UiRoot);
            var projectChangerUI = new ProjectSettingUI(globalNaviUI.UiRoot, saveSystem);
            var cityModelHandler = new CityModelHandler();
            var editBuilding = new EditBuilding(uxmlHandler.GetUxml(SubMenuUxmlType.EditBuilding));
            var gisDataLoaderUI = new GisDataLoaderUI(
                uxmlHandler.GetUxml(SubMenuUxmlType.Gis),
                saveSystem);
            
            var buildingSaveLoadSystem = new BuildingSaveLoadSystem();
            buildingSaveLoadSystem.SetEvent(saveSystem);

            var trafficSimulationManager = new TrafficSimulationManager(
                gameObject,
                roadNetworkManager,
                cityModel,
                humanFlowSystemBridge);
            var dashboardPanelUI = new DashboardPanelUI(
                uxmlHandler.GetUxml(SubMenuUxmlType.DashBoard),
                footerNaviUI,
                trafficSimulationManager);
            
            // 建物編集
            var buildingTrs = new BuildingTRSEditor(editBuilding,
                uxmlHandler.GetUxml(SubMenuUxmlType.EditBuilding),
                cameraManager.LandscapeCamera);
            
            // スピード調整
            var speedAdjuster = new SpeedControlUI(
                cameraManager.LandscapeCamera,
                cameraManager.ThirdPersonController);
            
            // モジュールをサブコンポーネントに追加
            subComponents = new List<ISubComponent>()
            {
                saveSystem,
                
                // アセット配置
                new ArrangementAsset(
                    uxmlHandler.GetUxml(SubMenuUxmlType.Asset),
                    saveSystem,
                    cameraManager.LandscapeCamera),
                
                // カメラ
                new LandscapeCameraUI(cameraManager.LandscapeCamera,
                    footerNaviUI.UiRoot,
                    uxmlHandler.Uxmls.ToArray()),
                new CameraPositionMemoryUI(
                    cameraManager.CameraPositionMemory,
                    uxmlHandler.Uxmls.ToArray(),
                    cameraManager.WalkerMoveByUserInput,
                    saveSystem,
                    globalNaviUI.UiRoot,
                    footerNaviUI.UiRoot),
                cameraManager.CameraMoveByUserInput,
                cameraManager.WalkerMoveByUserInput,
                
                cameraAutoRotate,
                
                new CameraAutoRotateUI(
                    cameraAutoRotate,
                    footerNaviUI.UiRoot),
                
                // 建物制御
                editBuilding,
                
                // 建物色制御
                new BuildingColorEditorUI(
                    new BuildingColorEditor(),
                    editBuilding,
                    uxmlHandler.GetUxml(SubMenuUxmlType.EditBuilding)),
                
                // 建物編集
                buildingTrs,
                
                // 天候制御
                new WeatherTimeEditorUI(
                    new WeatherTimeEditor(),
                    footerNaviUI.UiRoot,
                    TimeConstants.START_TIME,
                    TimeConstants.END_TIME
                ),
                
                // 高さ表示
                new VisualizeHeightUI(
                    new VisualizeHeight(),
                    footerNaviUI.UiRoot,
                    cameraManager.LandscapeCamera),
                
                // 歩行モード
                new WalkerModeUI(
                    uxmlHandler.GetUxml(SubMenuUxmlType.WalkMode),
                    cameraManager.LandscapeCamera,
                    cameraManager.WalkerMoveByUserInput),
                
                // 見通し解析
                new LineOfSight(saveSystem,
                    uxmlHandler.GetUxml(SubMenuUxmlType.Analytics)),
                
                // 建物高さ編集
                new BuildingHeightAdjustUI(uxmlHandler, buildingTrs),

                // 景観区域作成
                new PlanningUI(uxmlHandler.GetUxml(SubMenuUxmlType.Planning), globalNaviUI.UiRoot, CreateDbfFieldSettings()),
                
                // デザインコンペデータ表示
                new DesignCompDisplay(),
                
                // キー入力表示（デバッグ用）
                new KeyInputDisplayDebug(globalNaviUI.UiRoot),
            };
        }
        
        private void Start()
        {
            foreach (var c in subComponents)
            {
                c.Start();
            }
        }

        private void OnEnable()
        {
            foreach (var c in subComponents)
            {
                c.OnEnable();
            }
        }

        private void Update()
        {
            foreach (var c in subComponents)
            {
                c.Update(Time.deltaTime);
            }
        }

        private void LateUpdate()
        {
            foreach (var c in subComponents)
            {
                c.LateUpdate(Time.deltaTime);
            }
        }

        private void OnDisable()
        {
            foreach (var c in subComponents)
            {
                c.OnDisable();
            }
        }

        public static class TimeConstants
        {
            public const float START_TIME = 0.2917f;  // 7時
            public const float END_TIME = 0.834f;     // 20時
        }

        /// <summary>
        /// 景観区域プランニング用のDBFフィールドマッピング設定を作成
        /// </summary>
        private AreaPlanningDbfFieldSettings CreateDbfFieldSettings()
        {
            // デフォルト設定に追加のフィールド名を加える
            return new AreaPlanningDbfFieldSettings()
                .AddAreaNameField("固有名")
                .AddAreaNameField("種類");
        }
    }
}