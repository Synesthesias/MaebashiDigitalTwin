using PlateauToolkit.Sandbox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Reflection;


namespace Landscape2.Maebashi.Runtime.Dashboard
{
    /// <summary>
    /// 人アバターをスポーンするクラス
    /// </summary>
    public class HumanAvatarSpawner : MonoBehaviour
    {
        // 進行方向に対して垂直なオフセット
        [SerializeField]
        private float verticalOffsetScale = 1.0f;

        // オフセット幅に掛けるノイズのシード値
        [SerializeField]
        private float noiseSeed = 0.0f;
        [SerializeField]
        private float noiseScale = 0.05f;
        [SerializeField]
        private float spawnInteravalRandomScale = 0.05f;
        // オフセット幅に掛けるノイズのシード値 経過時間ベース　時間経過でスポーン位置をずらす用
        [SerializeField]
        private float timeSeed = 0.0f;

        // 1時間あたりのスポーン頻度
        [SerializeField]
        public int spawnFrequencyPerHour = (int)(3600.0f / 10.0f);  // 数値は適当　10秒に一度スポーンする頻度

        // アバターを破棄するまでの時間
        [SerializeField]
        public float destroyDelay = 10.0f;
        // 初期加速分の時間
        [SerializeField]
        public float destroyDelayExtra = 0.5f;

        [SerializeField]
        private HumanAvatarPoolingSystem humanAvatarPoolingSystem;
        [SerializeField]
        private Vector3 spawnPosition;
        [SerializeField]
        private Vector3 endPosition;
        [SerializeField]
        private PlateauSandboxTrack track;

        [SerializeField]
        private float preFreeSpace = 2.0f;
        [SerializeField]
        private float freeSpace = 3.5f;

        [SerializeField]
        // 歩道のシステム　あまりこの変数を使わないようにする。実装優先で依存関係が双方向。
        private HumanFlowCrosswalkSystem humanFlowCrosswalkSystem;

        private Coroutine spawnCoroutine;

        public void Initialize(Vector3 position, Vector3 endPos, PlateauSandboxTrack track, HumanAvatarPoolingSystem poolingSystem, HumanFlowCrosswalkSystem humanFlowCrosswalkSystem)
        {
            if (poolingSystem == null)
            {
                Debug.LogError("humanAvatarPoolingSystem is not assigned.");
                return;
            }
            this.humanAvatarPoolingSystem = poolingSystem;
            this.spawnPosition = position;
            this.endPosition = endPos;
            this.track = track;

            float avatorMoveSpd = 1.5f; // アバターの移動速度
            this.destroyDelay = track.CalcSplineLength() / avatorMoveSpd + destroyDelayExtra;
            destroyDelay *= 3.0f; // 到着後に消えるようになったのでピッタリである必要は無い。

            this.humanFlowCrosswalkSystem = humanFlowCrosswalkSystem;
        }

        public void StartSpawning()
        {
            if (spawnCoroutine == null)
            {
                spawnCoroutine = StartCoroutine(SpawnAvatars());
            }
        }

        public void StopSpawning()
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
        }

        private IEnumerator SpawnAvatars()
        {
            while (true)
            {
                // 一時間に0人だった場合スポーンしない。　ただし、spawnFrequencyPerHourの変更を検知するために一定秒数ごとに更新する
                if (spawnFrequencyPerHour <= 0)
                {
                    float updateFrequency = 5.0f;
                    yield return new WaitForSeconds(updateFrequency);
                    continue;
                }
                
                // 次のスポーンまで待機
                var spawnInterval = 1.0f / (spawnFrequencyPerHour / 3600.0f);
                var eff = spawnInterval * spawnInteravalRandomScale;    // 10per/2 
                spawnInterval = spawnInterval - Random.Range(-eff, eff);

                // アバターをスポーン
                var avatarId = Random.Range(0, humanAvatarPoolingSystem.CountAvatarKind() - 1);
                GameObject avatar = humanAvatarPoolingSystem.GetHumanAvatar(avatarId);
                if (avatar.TryGetComponent(out IPlateauSandboxPlaceableObject placeable))
                {
                    placeable.SetPosition(spawnPosition);
                }
                else
                {
                    avatar.transform.position = spawnPosition;
                }
                var trackMovement = AddPlateauSandboxTrackMovement(avatar, spawnPosition);
                AddHumanCrosswalkBehavoir(avatar, 
                    spawnPosition, endPosition,
                    trackMovement, humanFlowCrosswalkSystem);

                // 数秒後にアバターをプールに戻す　意図しない動作を起こした時用の処理　時間は十分に動作を完了出来る時間にする
                StartCoroutine(ReturnAvatarAfterDelay(avatar, destroyDelay));

                yield return new WaitForSeconds(spawnInterval);
            }
        }

        private void AddHumanCrosswalkBehavoir(
            GameObject avatar,
            Vector3 spawnPosition,
            Vector3 endPosition,
            PlateauSandboxTrackMovement trackMovement,
            HumanFlowCrosswalkSystem humanFlowCrosswalkSystem)
        {
            // 人アバターに歩道の挙動を追加する
            var crosswalkBehavior = avatar.AddComponent<HumanCrosswalkBehavior>();
            crosswalkBehavior.Initiliaze(
                spawnPosition,
                endPosition,
                preFreeSpace, freeSpace,
                trackMovement, track, humanFlowCrosswalkSystem, humanAvatarPoolingSystem);
        }

        /// <summary>
        /// 人アバターにトラック移動機能を追加する
        /// PlateauSandboxPlacementTool.ClickPlacement.cs MouseUp()を参考に
        /// </summary>
        /// <param name="obj"></param>
        private PlateauSandboxTrackMovement AddPlateauSandboxTrackMovement(GameObject obj, Vector3 spawnPosition)
        {
            // If placement mode is along tracks and the placed object is moveable, attach TrackMovement.
            PlateauSandboxTrackMovement trackMovement = obj.AddComponent<PlateauSandboxTrackMovement>();

            var trackOffset = Vector3.left * (verticalOffsetScale + Random.Range(-noiseSeed, noiseScale));
            SetPrivateField(trackMovement, trackOffset);

            // The target track is the track where the object was placed.
            trackMovement.Track = track;

            // The position of objects with TrackMovement is calculated by the interpolation value,
            // Then, calculate the normalized position along the track.
            track.GetNearestPoint(spawnPosition, out _, out int splineIndex, out float t);
            trackMovement.TrySetSplineContainerT(splineIndex + t);

            return trackMovement;
        }

        /// <summary>
        /// PlateauSandboxTrackMovementコンポーネントに設定を行う。
        /// privateなフィールドにアクセスするためにReflectionを使用
        /// </summary>
        /// <param name="trackMovement"></param>
        /// <param name="trackOffset"></param>
        private static void SetPrivateField(PlateauSandboxTrackMovement trackMovement, Vector3 trackOffset)
        {
            var trackOffsetProp = trackMovement.GetType().GetField("m_TrackOffset", BindingFlags.NonPublic | BindingFlags.Instance);
            trackOffsetProp.SetValue(trackMovement, trackOffset);

            var offsetProp = trackMovement.GetType().GetField("m_CollisionDetectOriginOffset", BindingFlags.NonPublic | BindingFlags.Instance);
            offsetProp.SetValue(trackMovement, Vector3.zero);

            var minCollisionDetectDistProp = trackMovement.GetType().GetField("m_MinCollisionDetectDistance", BindingFlags.NonPublic | BindingFlags.Instance);
            minCollisionDetectDistProp.SetValue(trackMovement, 0.2f);

            var detectRadius = trackMovement.GetType().GetField("m_CollisionDetectRadius", BindingFlags.NonPublic | BindingFlags.Instance);
            detectRadius.SetValue(trackMovement, 0.1f);

            var detectHeight = trackMovement.GetType().GetField("m_CollisionDetectHeight", BindingFlags.NonPublic | BindingFlags.Instance);
            detectHeight.SetValue(trackMovement, 1.0f);
        }

        /// <summary>
        /// アバターを指定した時間後にプールに戻す
        /// </summary>
        /// <param name="avatar"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        private IEnumerator ReturnAvatarAfterDelay(GameObject avatar, float delay)
        {
            yield return new WaitForSeconds(delay);
            humanAvatarPoolingSystem.ReturnHumanAvatar(avatar);
        }
    }

}
