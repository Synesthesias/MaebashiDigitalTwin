using UnityEngine;
using UnityEngine.UIElements;

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

        public DashboardPanelUI(VisualElement root)
        {
            simulationGroup = root.Q<RadioButtonGroup>("SimulationToggleGroup");
            SetupToggleEvents();
        }

        private void SetupToggleEvents()
        {
            simulationGroup.RegisterValueChangedCallback(evt =>
            {
                switch (evt.newValue)
                {
                    case 0:
                        OnHeatmapToggleChanged(true);
                        break;
                    case 1:
                        OnCarSimulationToggleChanged(true);
                        break;
                    case 2:
                        OnPeopleSimulationToggleChanged(true);
                        break;
                    default:
                        // すべてのシミュレーションをオフにする
                        OnHeatmapToggleChanged(false);
                        OnCarSimulationToggleChanged(false);
                        OnPeopleSimulationToggleChanged(false);
                        break;
                }
            });
        }

        private void OnHeatmapToggleChanged(bool isOn)
        {
            if (!isOn) return;
            // TODO: ヒートマップの表示/非表示を制御
            Debug.Log("Heatmap is enabled");
        }

        private void OnCarSimulationToggleChanged(bool isOn)
        {
            if (!isOn) return;
            // TODO: 自動車シミュレーションの表示/非表示を制御
            Debug.Log("Car simulation is enabled");
        }

        private void OnPeopleSimulationToggleChanged(bool isOn)
        {
            if (!isOn) return;
            // TODO: 人流シミュレーションの表示/非表示を制御
            Debug.Log("People simulation is enabled");
        }

        public void SetSimulationType(SimulationType type)
        {
            simulationGroup.value = (int)type;
        }

        public SimulationType GetCurrentSimulationType()
        {
            return (SimulationType)simulationGroup.value;
        }
    }

} 