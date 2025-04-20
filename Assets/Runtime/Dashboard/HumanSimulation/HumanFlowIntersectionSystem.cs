using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Landscape2.Maebashi.Runtime.Dashboard
{
    /// <summary>
    /// 交差点ごとに存在する人流シミュレーション機能
    /// </summary>
    public class HumanFlowIntersectionSystem : MonoBehaviour
    {
#if DEBUG
        [SerializeField]
        public bool isRegistered;
#endif

        // 歩道単位で存在する制御システム
        [SerializeField]
        private HumanFlowCrosswalkSystem[] humanFlowCrosswalkSystem;
    
        private HumanFlowData[] datas;

        public void Activate(bool isActivate)
        {
            // アクティブ化処理
            foreach (var item in humanFlowCrosswalkSystem)
            {
                item.Activate(isActivate);
            }
        }

        public void Initialize(HumanAvatarPoolingSystem poolingSystem)
        {
            // 人流量データの読み込み
            var loader = GetComponent<HumanFlowDataLoader>();
            datas = loader.HumanFlowDatas;

            if (datas == null) 
            {
                Debug.Log("data is null");
            }

            // 各交差点の制御システムをアクティブにする
            var crosswalkIdx = 0;
            foreach (var crosswalkSystem in humanFlowCrosswalkSystem)
            {
#if DEBUG
                crosswalkSystem.isRegistered = true;
#endif
                crosswalkSystem.Initialize(poolingSystem, datas[0], crosswalkIdx);
                crosswalkIdx++;
            }
        }

        public void SetDataId(int id)
        {
            foreach (var crosswalkSystem in humanFlowCrosswalkSystem)
            {
                crosswalkSystem.SetData(datas[id]);
            }
        }

        public void SetTime(int time)
        {
            foreach (var crosswalkSystem in humanFlowCrosswalkSystem)
            {
                crosswalkSystem.SetTime(time);
            }
        }

        /// <summary>
        /// デバッグ用　すべての信号機の状態を変更する
        /// </summary>
        /// <param name="isRed"></param>
        public void Debug_SetSignalState(bool isRed)
        {
            foreach (var crosswalkSystem in humanFlowCrosswalkSystem)
            {
                crosswalkSystem.SetSignalState(isRed);
            }
        }

    }

}
