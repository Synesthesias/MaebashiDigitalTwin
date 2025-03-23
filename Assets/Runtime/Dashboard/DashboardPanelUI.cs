using Landscape2.Maebashi.Runtime.Dashboard;
using UnityEngine;
using UnityEngine.UIElements;
using TrafficSimulationTool.Runtime;
using System.Threading.Tasks;
using PLATEAU.CityInfo;

namespace Landscape2.Maebashi.Runtime
{
    public enum SimulationType
    {
        None = -1,
        Heatmap = 0,
        CarSimulation = 1,
        PeopleSimulation = 2
    }
    
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
                if (evt.newValue)
                {
                    carSimulationToggle.value = false;
                    peopleSimulationToggle.value = false;
                }
            });

            carSimulationToggle.RegisterValueChangedCallback(evt =>
            {
                OnCarSimulationToggleChanged(evt.newValue);
                if (evt.newValue)
                {
                    heatmapToggle.value = false;
                    peopleSimulationToggle.value = false;
                }
            });

            peopleSimulationToggle.RegisterValueChangedCallback(evt =>
            {
                OnPeopleSimulationToggleChanged(evt.newValue);
                if (evt.newValue)
                {
                    heatmapToggle.value = false;
                    carSimulationToggle.value = false;
                }
            });
        }

        private void OnHeatmapToggleChanged(bool isOn)
        {
            if (trafficSimulationManager.HeatmapManager != null)
            {
                trafficSimulationManager.HeatmapManager.SetHeatmapEnabled(isOn);
                if (isOn)
                {
                    trafficSimulationManager.UpdateBasedOnTime(timeValue);
                }
            }
            else
            {
                Debug.LogWarning("TrafficHeatmapManager is not available.");
            }
        }

        private void OnCarSimulationToggleChanged(bool isOn)
        {
            if (!isOn) return;
            Debug.Log("Car simulation is enabled");
        }

        private void OnPeopleSimulationToggleChanged(bool isOn)
        {
            if (!isOn) return;
            Debug.Log("People simulation is enabled");
        }

        public void SetSimulationType(SimulationType type)
        {
            simulationGroup.value = (int)type;
        }
    }
} 