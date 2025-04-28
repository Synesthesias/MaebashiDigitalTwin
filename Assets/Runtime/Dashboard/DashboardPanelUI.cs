using Landscape2.Maebashi.Runtime.Dashboard;
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
        private TrafficSimulationManager trafficSimulationManager;
        private FooterNaviUI footerNaviUI;
        private float timeValue;
        
        public DashboardPanelUI(VisualElement root, FooterNaviUI footerNaviUI, TrafficSimulationManager trafficSimulationManager)
        {
            this.root = root;
            this.trafficSimulationManager = trafficSimulationManager;
            this.footerNaviUI = footerNaviUI;
            simulationGroup = root.Q<RadioButtonGroup>("SimulationGroup");
            var heatmapToggle = root.Q<Toggle>("HeatmapIcon");
            var carSimulationToggle = root.Q<Toggle>("CarSimulationIcon");
            var peopleSimulationToggle = root.Q<Toggle>("PeopleSimulationIcon");
            
            footerNaviUI.OnTimeChanged.AddListener((value) =>
            {
                timeValue = value;
                trafficSimulationManager.UpdateBasedOnTime(timeValue);
            });
            
            SetupToggleEvents();
        }

        private void SetupToggleEvents()
        {
            var heatmapToggle = root.Q<Toggle>("HeatmapIcon");
            var carSimulationToggle = root.Q<Toggle>("CarSimulationIcon");
            var peopleSimulationToggle = root.Q<Toggle>("PeopleSimulationIcon");

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