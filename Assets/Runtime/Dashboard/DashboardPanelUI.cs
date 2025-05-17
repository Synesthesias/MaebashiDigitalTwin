using Landscape2.Maebashi.Runtime.Dashboard;
using Landscape2.Runtime.UiCommon;
using UnityEngine;
using UnityEngine.UIElements;
using TrafficSimulationTool.Runtime;
using System.Threading.Tasks;
using PLATEAU.CityInfo;

namespace Landscape2.Maebashi.Runtime
{
    public class DashboardPanelUI
    {
        private RadioButtonGroup simulationGroup;
        private VisualElement root;
        private VisualElement heatMapExample;
        private TrafficSimulationManager trafficSimulationManager;
        private FooterNaviUI footerNaviUI;
        private float timeValue;
        
        private const string HeatMapExampleViewName = "HeatMapExampleView";
        
        public DashboardPanelUI(VisualElement root, FooterNaviUI footerNaviUI, TrafficSimulationManager trafficSimulationManager)
        {
            this.root = root;
            this.trafficSimulationManager = trafficSimulationManager;
            this.footerNaviUI = footerNaviUI;
            simulationGroup = root.Q<RadioButtonGroup>("SimulationGroup");
            var heatmapToggle = root.Q<Toggle>("HeatmapIcon");
            var carSimulationToggle = root.Q<Toggle>("CarSimulationIcon");
            var peopleSimulationToggle = root.Q<Toggle>("PeopleSimulationIcon");

            CreateHeatmapExample();
            
            footerNaviUI.OnTimeChanged.AddListener((value) =>
            {
                timeValue = value;
                trafficSimulationManager.UpdateBasedOnTime(timeValue);
            });
            
            SetupEvents();
        }

        private void CreateHeatmapExample()
        {
            heatMapExample = new UIDocumentFactory().CreateWithUxmlName(HeatMapExampleViewName);
            var bar = heatMapExample.Q<VisualElement>("ExampleColorBar");
            bar.style.backgroundImage = new StyleBackground(ColorManipulator.GenerateGradientTexture());
            var heatMapGameObject = GameObject.Find(HeatMapExampleViewName);
            if (heatMapGameObject != null)
            {
                var uiDocument = heatMapGameObject.GetComponent<UIDocument>();
                if (uiDocument != null)
                {
                    uiDocument.sortingOrder = -1;
                }
                else
                {
                    Debug.LogWarning($"UIDocument component not found on GameObject '{HeatMapExampleViewName}'.");
                }
            }
            else
            {
                Debug.LogWarning($"GameObject with name '{HeatMapExampleViewName}' not found.");
            }
            heatMapExample.Hide();
        }

        private void SetupEvents()
        {
            var dateDropdown = root.Q<DropdownField>("DateDropdown");
            var heatmapToggle = root.Q<Toggle>("HeatmapIcon");
            var carSimulationToggle = root.Q<Toggle>("CarSimulationIcon");
            var peopleSimulationToggle = root.Q<Toggle>("PeopleSimulationIcon");

            dateDropdown.RegisterValueChangedCallback(evt =>
            {
                // TODO: 日付切り替え
            });
            
            heatmapToggle.RegisterValueChangedCallback(evt =>
            {
                OnHeatmapToggleChanged(evt.newValue);
            });

            carSimulationToggle.RegisterValueChangedCallback(evt =>
            {
                OnCarSimulationToggleChanged(evt.newValue);
            });

            peopleSimulationToggle.RegisterValueChangedCallback(evt =>
            {
                OnPeopleSimulationToggleChanged(evt.newValue);
            });
        }

        private void OnHeatmapToggleChanged(bool isOn)
        {
            if (trafficSimulationManager.HeatmapManager != null)
            {
                trafficSimulationManager.HeatmapManager.SetHeatmapEnabled(isOn);
                if (isOn)
                {
                    trafficSimulationManager.UpdateTimeline(timeValue);
                    heatMapExample.Show();
                }
                else
                {
                    heatMapExample.Hide();
                }
            }
            else
            {
                Debug.LogWarning("TrafficHeatmapManager is not available.");
            }
        }

        private void OnCarSimulationToggleChanged(bool isOn)
        {
            trafficSimulationManager.CarSimulationManager?.SetCarSimulationEnabled(isOn);
        }

        private void OnPeopleSimulationToggleChanged(bool isOn)
        {
            trafficSimulationManager.HumanFlowSystemBridge.Activate(isOn);
        }
    }
} 