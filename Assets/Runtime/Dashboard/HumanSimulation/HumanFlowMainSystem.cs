using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using PlateauToolkit.Sandbox;

namespace Landscape2.Maebashi.Runtime.Dashboard
{
    /// <summary>
    /// 人流シミュレーションのメインシステム
    /// </summary>
    public class HumanFlowMainSystem : MonoBehaviour
    {
        // 交差点ごとに存在する制御システム
        [SerializeField]
        private HumanFlowIntersectionSystem[] humanFlowIntersectionSystem;

        [SerializeField]
        private HumanAvatarPoolingSystem poolingSystem;

        public void Initialize()
        {
            if (poolingSystem == null)
            {
                Debug.LogError("HumanAvatarPoolingSystem is not assigned.");
                return;
            }

            foreach (var item in humanFlowIntersectionSystem)
            {
#if DEBUG
                item.isRegistered = true;
#endif
                item.Initialize(poolingSystem);
            }

        }

        public void Activate(bool isActivate)
        {
            // アクティブ化処理
            foreach (var item in humanFlowIntersectionSystem)
            {
                item.Activate(isActivate);
            }
        }

        /// <summary>
        /// 各横断歩道に対して信号機を設定する
        /// </summary>
        /// <param name="signalStates"></param>
        public void SetSignalState(Dictionary<object, string> signalStates)
        {
            // 信号機の状態を設定する処理
        }

        /// <summary>
        /// すべての信号機に同じ設定する
        /// </summary>
        /// <param name="isRed"></param>
        public void Debug_SetSignalState(bool isRed)
        {
            // 信号機の状態を設定する処理
            foreach (var item in humanFlowIntersectionSystem)
            {
                item.Debug_SetSignalState(isRed);
            }
        }

        /// <summary>
        /// 利用するデータIDを設定する
        /// </summary>
        /// <param name="dataId"></param>
        public void SetDataId(int dataId)
        {
            foreach (var item in humanFlowIntersectionSystem)
            {
                item.SetDataId(dataId);
            }
        }

        /// <summary>
        /// 時間を設定する
        /// </summary>
        /// <param name="time"></param>
        public void SetTime(int time)
        {
            // 時間帯を設定する処理
            foreach (var item in humanFlowIntersectionSystem)
            {
                item.SetTime(time);
            }
        }
    }

}
