using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PlateauToolkit.Sandbox;
using System;
using UnityEngine.Splines;
using System.Linq;

namespace Landscape2.Maebashi.Runtime.Dashboard
{
    /// <summary>
    /// 歩道ごとに存在する人流シミュレーション機能
    /// </summary>
    public class HumanFlowCrosswalkSystem : MonoBehaviour
    {
#if DEBUG
        [SerializeField]
        public bool isRegistered;
#endif

        [ContextMenu("generate reverse spline")]
        private void GenerateReverseSpline()
        {
            // トラックAを元にトラックBを生成する
            if (trackA == null || trackB != null)
            {
                Debug.LogError("Invalid state: trackA must not be null and trackB must be null");
                return;
            }

            // ついでにtrackAの名前も変更する
            trackA.name = this.name + "_track";

            var obj = Instantiate(trackA);
            var revSplineContainer = obj.GetComponent<SplineContainer>();
            ReverseSpline(revSplineContainer);
            trackB = obj;
            trackB.name = trackA.name + "_rev";

        }

        public struct SpawnerSet
        {
            public HumanAvatarSpawner spawnerA;    // スプライン始点に生成される
            public HumanAvatarSpawner spawnerB;    // スプライン終点に生成される
        }

        private HumanFlowData humanFlowData;
        private int crosswalkIdx;

        [SerializeField]
        private GameObject trackA;
        [SerializeField]
        private GameObject trackB;

        // 歩道橋か　歩道橋の場合は信号の影響を受けない
        [SerializeField]
        private bool isFootbridge = false;

        private bool isRed;

        private SpawnerSet spawnerSet;

        [SerializeField]
        int currentTime = 0; // 時間帯の設定 (7-19 時間)


        public void Initialize(HumanAvatarPoolingSystem poolingSystem, HumanFlowData humanFlowData, int crosswalkIdx)
        {
            this.humanFlowData = humanFlowData;
            this.crosswalkIdx = crosswalkIdx;

            // スポナーの生成
            GenerateSpawner(poolingSystem, out spawnerSet);

        }

        public void Activate(bool isActivate)
        {
            if (isActivate)
            {
                if (isRed)
                {
                    return;
                }

                StartSpawning();
            }
            else
            {
                StopSpawning();
            }
        }

        /// <summary>
        /// 参照するデータの変更
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool SetData(HumanFlowData data)
        {
            this.humanFlowData = data;
            SetTime(currentTime);
            return true;
        }

        /// <summary>
        /// 時間帯の設定
        /// </summary>
        public bool SetTime(int time)
        {
            time = Mathf.Clamp(time, HumanFlowData.DATA_TIME_START, HumanFlowData.DATA_TIME_END - 1);
            currentTime = time;
            // スポナーに時間帯を設定
            if (spawnerSet.spawnerA != null)
            {
                spawnerSet.spawnerA.spawnFrequencyPerHour = humanFlowData.Read(crosswalkIdx * 2, time);
            }
            if (spawnerSet.spawnerB != null)
            {
                spawnerSet.spawnerB.spawnFrequencyPerHour = humanFlowData.Read(crosswalkIdx * 2 + 1, time);
            }
            return true;
        }

        public void SetSignalState(bool isRed)
        {
            if (isFootbridge)
            {
                return;
            }

            if (isRed)
            {
                StopSpawning();
            }
            else
            {
                StartSpawning();
            }
        }

        /// <summary>
        /// スポーンを開始する
        /// </summary>
        private void StartSpawning()
        {
            spawnerSet.spawnerA?.StartSpawning();
            spawnerSet.spawnerB?.StartSpawning();
        }

        private void StopSpawning()
        {
            spawnerSet.spawnerA?.StopSpawning();
            spawnerSet.spawnerB?.StopSpawning();
        }

        private void GenerateSpawner(HumanAvatarPoolingSystem poolingSystem, out SpawnerSet res)
        {
            var spawnerSet = new SpawnerSet();
            
            var spawnerA = trackA.AddComponent<HumanAvatarSpawner>();
            spawnerA.Initialize(CalcSpawnerPosition(trackA), trackA.GetComponent<PlateauSandboxTrack>(), poolingSystem);
            var spawnerB = trackB.AddComponent<HumanAvatarSpawner>();
            spawnerB.Initialize(CalcSpawnerPosition(trackB), trackB.GetComponent<PlateauSandboxTrack>(), poolingSystem);

            spawnerSet = new SpawnerSet { spawnerA = spawnerA, spawnerB = spawnerB };

            res = spawnerSet;
        }

        private Vector3 CalcSpawnerPosition(GameObject track)
        {
            SplineContainer splineContainer = track.GetComponent<SplineContainer>();
            if (splineContainer == null)
            {
                Debug.LogError("SplineContainer is not found on this GameObject.");
                return Vector3.zero;
            }

            return splineContainer.Spline.Knots.First().Position;

        }

        private void ReverseSpline(SplineContainer splineContainer)
        {
            if (splineContainer == null)
            {
                Debug.LogError("SplineContainer is not assigned.");
                return;
            }

            Spline spline = splineContainer.Spline;
            splineContainer.ReverseFlow(0);
        }
    }

}
