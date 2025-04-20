using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Landscape2.Maebashi.Runtime.Dashboard
{

    /// <summary>
    /// 時間帯、信号機の状態、アクティブ化を
    /// HumanFlowMainSystemに伝達するクラス
    /// </summary>
    [RequireComponent(typeof(HumanFlowMainSystem))]

    public class TrafficSystemMediatorForHumanFlow : MonoBehaviour
    {
        [SerializeField]
        private int dataId = 0;
        private int currentDataId;
        
        [SerializeField]
        private int time = 0;
        private int currentTime;

        [SerializeField]
        private bool isDebugStop = false;
        private bool currentDebugStop;

        HumanFlowMainSystem humanFlowMainSystem;

        public void SetTime(float time)
        {
            this.time = (int)(time * 24.0f);

            if (currentTime != this.time)
            {
                currentTime = this.time;
                humanFlowMainSystem.SetTime(this.time);
            }
        }

        public void Activate(bool isActivate)
        {
            humanFlowMainSystem.Activate(isActivate);
        }

        public void Initialize()
        {
            currentDataId = dataId;
            currentTime = time;
            currentDebugStop = isDebugStop;

            humanFlowMainSystem = GetComponent<HumanFlowMainSystem>();
            humanFlowMainSystem.Initialize();
        }

        // Update is called once per frame
        void Update()
        {
            if (currentDataId != dataId)
            {
                currentDataId = dataId;
                humanFlowMainSystem.SetDataId(dataId);
            }

            //if (currentTime != time)
            //{
            //    currentTime = time;
            //    humanFlowMainSystem.SetTime(time);
            //}

            if (currentDebugStop != isDebugStop)
            {
                currentDebugStop = isDebugStop;
                humanFlowMainSystem.Debug_SetSignalState(isDebugStop);
            }

        }


    }

}

