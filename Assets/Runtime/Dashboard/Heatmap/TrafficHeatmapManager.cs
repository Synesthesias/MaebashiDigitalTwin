using UnityEngine;

namespace Landscape2.Maebashi.Runtime.Dashboard
{
    /// <summary>
    /// 交通量ヒートマップの表示を管理するクラス
    /// </summary>
    public class TrafficHeatmapManager
    {
        private bool isHeatmapEnabled;
        public bool IsHeatmapEnabled => isHeatmapEnabled;
        private GameObject heatMapRoadObject;
        
        public TrafficHeatmapManager()
        {
            // HeatMapRoadオブジェクトの取得
            heatMapRoadObject = GameObject.Find("HeatMapRoad");
            if (heatMapRoadObject == null)
            {
                Debug.LogWarning("HeatMapRoadオブジェクトが見つかりません。");
                return; // オブジェクトが見つからない場合は早期リターン
            }
            
            // 初期状態は非表示
            heatMapRoadObject.SetActive(false);
        }

        public void SetHeatmapEnabled(bool enabled)
        {
            isHeatmapEnabled = enabled;
            if (heatMapRoadObject != null)
            {
                heatMapRoadObject.SetActive(enabled);
            }
            else
            {
                Debug.LogWarning("HeatMapRoadオブジェクトが存在しないため、表示状態を変更できません。");
            }
        }
    }
} 