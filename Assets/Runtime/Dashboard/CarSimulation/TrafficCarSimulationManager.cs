using UnityEngine;

namespace Landscape2.Maebashi.Runtime
{
    public class TrafficCarSimulationManager
    {
        private bool isCarSimulationEnabled = false;
        private GameObject trafficManagerObject;

        public TrafficCarSimulationManager()
        {
            // TrafficManagerオブジェクトの取得
            trafficManagerObject = GameObject.Find("TrafficManager");
            if (trafficManagerObject == null)
            {
                Debug.LogWarning("trafficManagerObjectオブジェクトが見つかりません。");
                return;
            }
            
            // 初期状態は非表示
            trafficManagerObject.SetActive(false);
        }

        public void SetCarSimulationEnabled(bool enabled)
        {
            isCarSimulationEnabled = enabled;
            trafficManagerObject?.SetActive(enabled);
        }
    }
}