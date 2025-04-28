using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlateauToolkit.Sandbox;
using UnityEngine.Splines;

namespace Landscape2.Maebashi.Runtime.Dashboard
{
    public class HumanCrosswalkBehavior : MonoBehaviour
    {
        // 歩道の手前、奥側の　信号に歩道が掛かっていないスペース
        private float crosswalkFreeSpace = 10.0f;    // m
        private float crosswalkPreFreeSpace = 5.0f;    // m
        private Vector3 startPos;
        private Vector3 endPos;

        private PlateauSandboxTrackMovement trackMovement;
        private PlateauSandboxTrack track;
        private HumanFlowCrosswalkSystem humanFlowCrosswalkSystem;
        private HumanAvatarPoolingSystem poolingSystem;

        public void Initiliaze(
            Vector3 startPos,
            Vector3 endPos, 
            float preSpace,
            float space, 
            PlateauSandboxTrackMovement trackMovement, 
            PlateauSandboxTrack track, 
            HumanFlowCrosswalkSystem humanFlowCrosswalkSystem,
            HumanAvatarPoolingSystem poolingSystem)
        {
            this.startPos = startPos;
            this.endPos = endPos;
            this.crosswalkFreeSpace = space;
            this.crosswalkPreFreeSpace = preSpace;
            this.trackMovement = trackMovement;
            this.track = track;
            this.humanFlowCrosswalkSystem = humanFlowCrosswalkSystem;
            this.poolingSystem = poolingSystem;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            var dis = (this.gameObject.transform.position - startPos).sqrMagnitude;
            var isFrontFreeSpace = dis < crosswalkFreeSpace * crosswalkFreeSpace;
            var isPreFrontFreeSpace = dis < crosswalkPreFreeSpace * crosswalkPreFreeSpace;

            var dis2 = (endPos - this.gameObject.transform.position).sqrMagnitude;
            var isBackFreeSpace = dis2 < crosswalkPreFreeSpace * crosswalkPreFreeSpace;

            // 止まっていて緑信号なら進            
            if (trackMovement.IsMoving == false)
            {
                if (humanFlowCrosswalkSystem.LightState == HumanFlowCrosswalkSystem.TrafficLightState.Green)
                {
                    trackMovement.StartRandomWalk();
                }
            }

            // 歩道の手前
            if (isFrontFreeSpace)
            {
                if (humanFlowCrosswalkSystem.LightState == HumanFlowCrosswalkSystem.TrafficLightState.Red)
                {

                    if (!isPreFrontFreeSpace)
                    {
                        // stop
                        if (trackMovement.IsMoving)
                        {
                            trackMovement.Stop();
                        }
                    }
                }
            }
            // 歩道の奥側
            else if (isBackFreeSpace)
            {
                // return pool
                poolingSystem.ReturnHumanAvatar(this.gameObject);
            }
            // 歩道上
            else
            {
                if (humanFlowCrosswalkSystem.LightState == HumanFlowCrosswalkSystem.TrafficLightState.Red)
                {
                    // return pool
                    poolingSystem.ReturnHumanAvatar(this.gameObject);
                }
            }
        }
    }
}
