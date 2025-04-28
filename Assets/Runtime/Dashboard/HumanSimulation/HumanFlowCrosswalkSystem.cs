using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PlateauToolkit.Sandbox;
using System;
using UnityEngine.Splines;
using System.Linq;
using AWSIM;

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

        // 歩行者信号の色
        [Serializable]
        public enum TrafficLightState
        {
            Red,
            Green
        }
        [SerializeField]
        TrafficLightState trafficLightState = TrafficLightState.Green;
        public TrafficLightState LightState => trafficLightState;


        [ContextMenu("Setup system")]
        private void SetupSystem()
        {
            SplitSpline();
            GenerateReverseSplines();

            SetupSpawner();
        }

        private void SplitSpline()
        {
            if (trackAGroup.Length == 0)
                return;
            GameObject templateObj = trackAGroup[0];
            List<GameObject> addObjs = new List<GameObject>(trackAGroup.Length);
            List<GameObject> removeObjs = new List<GameObject>(trackAGroup.Length);
            List<Spline> splitSplines = new List<Spline>(5);   // cap はテキトウ
            foreach (var item in trackAGroup)
            {
                var splineContaienr = item.GetComponent<SplineContainer>();
                var splines = splineContaienr.Splines;
                var nSpline = splines.Count();
                // SplineContainerに複数設定されている場合　分割して別のオブジェクトにする　後続する処理に対応するため
                if (nSpline <= 1)
                {
                    addObjs.Add(item);
                    continue;
                }
                removeObjs.Add(item);
                foreach (var spline in splines)
                {
                    splitSplines.Add(spline);
                }
            }

            var nObj = addObjs.Count + splitSplines.Count;
            List<GameObject> newList = new List<GameObject>(nObj);
            newList.AddRange(addObjs);
            int i = 0;
            foreach (var item in splitSplines)
            {
                var obj = Instantiate(templateObj);
                obj.name = this.name + "_track" + i++;
                obj.transform.parent = templateObj.transform.parent;

                newList.Add(obj);
                var splineContainer = obj.GetComponent<SplineContainer>();
                splineContainer.Splines = null;
                splineContainer.AddSpline(item);
            }


            trackAGroup = newList.ToArray();

            // 古いTrackを削除
            foreach (var item in removeObjs)
            {
                DestroyImmediate(item);
            }
        }

        [ContextMenu("generate reverse spline")]
        private void GenerateReverseSplines()
        {
            if (trackBGroup.Length != trackAGroup.Length)
            {
                trackBGroup = new GameObject[trackAGroup.Length];
            }
            for (int i = 0; i < trackAGroup.Length; i++)
            {
                trackBGroup[i] = GenerateReverseSpline(i, trackAGroup[i], trackBGroup[i]);
            }   
        }

        private GameObject GenerateReverseSpline(int i, GameObject trackA, GameObject trackB)
        {
            // トラックAを元にトラックBを生成する
            if (trackA == null || trackB != null)
            {
                Debug.LogError("Invalid state: trackA must not be null and trackB must be null");
                return null;
            }

            if (trackA.GetComponent<SplineContainer>() == null)
            {
                Debug.LogError("SplineContainer is not found on this GameObject.");
                return null;
            }

            // ついでにtrackAの名前も変更する
            trackA.name = this.name + "_track" + i;

            var obj = Instantiate(trackA);
            var revSplineContainer = obj.GetComponent<SplineContainer>();
            ReverseSpline(revSplineContainer);
            trackB = obj;
            trackB.name = trackA.name + "_rev";
            trackB.transform.parent = trackA.transform.parent;
            return trackB;

        }

        [ContextMenu("setup spawner")]
        private void SetupSpawner()
        {
            // スポナーの生成
            // HumanAvatarPoolingSystemを持つゲームオブジェクトを検索する
            var poolingSystem = FindObjectOfType<HumanAvatarPoolingSystem>();
            if (poolingSystem == null)
            {
                Debug.LogError("No GameObject with HumanAvatarPoolingSystem found in the scene.");
            }

            if (trackAGroup.Length != trackBGroup.Length)
            {
                Debug.LogError("TrackA and TrackB must have the same length.");
                return;
            }

            spawnerSetGroup = new SpawnerSet[trackAGroup.Length];
            SpawnerSet spawnerSet;
            for(int i = 0; i < trackAGroup.Length; i++)
            {
                GenerateSpawner(poolingSystem, trackAGroup[i], trackBGroup[i], out spawnerSet);
                spawnerSetGroup[i] = spawnerSet;
            }

        }

        [Serializable]
        public struct SpawnerSet
        {
            public HumanAvatarSpawner spawnerA;    // スプライン始点に生成される
            public HumanAvatarSpawner spawnerB;    // スプライン終点に生成される
        }

        private HumanFlowData humanFlowData;
        private int crosswalkIdx;

        [SerializeField]
        private GameObject[] trackAGroup;
        [SerializeField]
        private GameObject[] trackBGroup;
        [SerializeField]
        private float crosswalkWidth = 3.0f / 2.0f;

        // 参照先の信号　参照先が無い場合は信号の影響を受けない。歩道橋などは設定しない。
        [SerializeField]
        private TrafficLight trafficLight = null;

        [SerializeField]
        private SpawnerSet[] spawnerSetGroup;

        [SerializeField]
        int currentTime = 0; // 時間帯の設定 (7-19 時間)


        public void Initialize(HumanFlowData humanFlowData, int crosswalkIdx)
        {
            this.humanFlowData = humanFlowData;
            this.crosswalkIdx = crosswalkIdx;

            if (trafficLight != null)
            {
                trafficLight.evOnChangedBulbColor += OnChangedTrafficLightColor;
            }

        }

        private void OnChangedTrafficLightColor(in Color col)
        {
            float th = 0.1f;
            if (CalcColorDifference(col, Color.red) < th)
            {
                trafficLightState = TrafficLightState.Green;
            }
            else if (CalcColorDifference(col, Color.green) < th)
            {
                trafficLightState = TrafficLightState.Red;
            }
            else if (CalcColorDifference(col, Color.yellow) < th)
            {
                trafficLightState = TrafficLightState.Red;
            }

            float CalcColorDifference(Color a, Color b)
            {
                return Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b);
            }
        }

        public void Activate(bool isActivate)
        {
            if (isActivate)
            {
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
            var nLine = spawnerSetGroup.Length;

            time = Mathf.Clamp(time, HumanFlowData.DATA_TIME_START, HumanFlowData.DATA_TIME_END - 1);
            currentTime = time;
            var amountA = humanFlowData.Read(crosswalkIdx * 2, time) / nLine;
            var amountB = humanFlowData.Read(crosswalkIdx * 2 + 1, time) / nLine;

            // スポナーに時間帯を設定
            foreach (var set in spawnerSetGroup)
            {
                SetSpawnFrequencyPerHer(set, amountA, amountB);
            }
            return true;

            void SetSpawnFrequencyPerHer(in SpawnerSet set, int amountA, int amountB)
            {
                if (set.spawnerA != null)
                {
                    set.spawnerA.spawnFrequencyPerHour = amountA;
                }
                if (set.spawnerB != null)
                {
                    set.spawnerB.spawnFrequencyPerHour = amountB;
                }
            }
        }

        public void SetSignalState(bool isRed)
        {
            if (trafficLight == null)
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
            foreach (var set in spawnerSetGroup)
            {
                StartSpawning(set);
            }

            void StartSpawning(in SpawnerSet set)
            {
                set.spawnerA?.StartSpawning();
                set.spawnerB?.StartSpawning();
            }
        }

        private void StopSpawning()
        {
            foreach (var set in spawnerSetGroup)
            {
                StopSpawning(set);
            }

            void StopSpawning(in SpawnerSet set)
            {
                set.spawnerA?.StopSpawning();
                set.spawnerB?.StopSpawning();
            }
        }

        private void GenerateSpawner(HumanAvatarPoolingSystem poolingSystem, GameObject trackA, GameObject trackB, out SpawnerSet res)
        {
            var spawnerSet = new SpawnerSet();
            var spawnerA = InitializeSpawner(poolingSystem, trackA);
            var spawnerB = InitializeSpawner(poolingSystem, trackB);
            spawnerSet = new SpawnerSet { spawnerA = spawnerA, spawnerB = spawnerB };

            res = spawnerSet;

        }

        private HumanAvatarSpawner InitializeSpawner(HumanAvatarPoolingSystem poolingSystem, GameObject track)
        {
            var spawner = track.GetComponent<HumanAvatarSpawner>();
            if (spawner == null)
            {
                spawner = track.AddComponent<HumanAvatarSpawner>();
            }
            spawner.Initialize(
                CalcSpawnerPosition(track), CalcEndPosition(track), track.GetComponent<PlateauSandboxTrack>(), poolingSystem, this);

            return spawner;
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

        private Vector3 CalcEndPosition(GameObject track)
        {
            SplineContainer splineContainer = track.GetComponent<SplineContainer>();
            if (splineContainer == null)
            {
                Debug.LogError("SplineContainer is not found on this GameObject.");
                return Vector3.zero;
            }

            return splineContainer.Spline.Knots.Last().Position;

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
