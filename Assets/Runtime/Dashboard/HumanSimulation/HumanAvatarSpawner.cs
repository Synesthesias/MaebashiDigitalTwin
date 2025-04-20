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
        private float verticalOffsetScale = 3.0f;

        // オフセット幅に掛けるノイズのシード値
        [SerializeField]
        private float noiseSeed = 0.0f;
        // オフセット幅に掛けるノイズのシード値 経過時間ベース　時間経過でスポーン位置をずらす用
        [SerializeField]
        private float timeSeed = 0.0f;

        // 1時間あたりのスポーン頻度
        [SerializeField]
        public float spawnFrequencyPerHour = 3600.0f / 10.0f;  // 数値は適当　10秒に一度スポーンする頻度

        // 赤信号の時間比率(簡易実装版 本来はデータから算出するべき)
        [SerializeField]
        public float redLightDurationRatio = 0.0f;

        // アバターを破棄するまでの時間
        [SerializeField]
        public float destroyDelay = 10.0f; 

        private HumanAvatarPoolingSystem humanAvatarPoolingSystem;
        private Vector3 spawnPosition;
        private PlateauSandboxTrack track;

        private Coroutine spawnCoroutine;

        public void Initialize(Vector3 position, PlateauSandboxTrack track, HumanAvatarPoolingSystem poolingSystem)
        {
            if (poolingSystem == null)
            {
                Debug.LogError("humanAvatarPoolingSystem is not assigned.");
                return;
            }
            humanAvatarPoolingSystem = poolingSystem;
            this.spawnPosition = position;
            this.track = track;
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
                AddPlateauSandboxTrackMovement(avatar);

                // 数秒後にアバターをプールに戻す　意図しない動作を起こした時用の処理　時間は十分に動作を完了出来る時間にする
                StartCoroutine(ReturnAvatarAfterDelay(avatar, destroyDelay));

                // 次のスポーンまで待機
                var spawnInterval = 1.0f / (spawnFrequencyPerHour / 3600.0f);
                // 赤信号の時間を考慮してスポーン間隔を調整　赤信号の割合50%なら緑の時に倍の量スポーンする必要がある。
                if (redLightDurationRatio > 0.0f && redLightDurationRatio < 1.0f)
                {
                    spawnInterval /= (1.0f - redLightDurationRatio);
                }
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        /// <summary>
        /// 人アバターにトラック移動機能を追加する
        /// PlateauSandboxPlacementTool.ClickPlacement.cs MouseUp()を参考に
        /// </summary>
        /// <param name="obj"></param>
        private void AddPlateauSandboxTrackMovement(GameObject obj)
        {
            // If placement mode is along tracks and the placed object is moveable, attach TrackMovement.
            PlateauSandboxTrackMovement trackMovement = obj.AddComponent<PlateauSandboxTrackMovement>();

            var trackOffset = Vector3.right * verticalOffsetScale * Mathf.PerlinNoise1D(noiseSeed);
            SetPrivateField(trackMovement, trackOffset);

            // The target track is the track where the object was placed.
            trackMovement.Track = track;

            // The position of objects with TrackMovement is calculated by the interpolation value,
            // Then, calculate the normalized position along the track.
            track.GetNearestPoint(spawnPosition, out _, out int splineIndex, out float t);
            trackMovement.TrySetSplineContainerT(splineIndex + t);

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
            minCollisionDetectDistProp.SetValue(trackMovement, 0.0f);

            var detectRadius = trackMovement.GetType().GetField("m_CollisionDetectRadius", BindingFlags.NonPublic | BindingFlags.Instance);
            detectRadius.SetValue(trackMovement, 0.0f);

            var detectHeight = trackMovement.GetType().GetField("m_CollisionDetectHeight", BindingFlags.NonPublic | BindingFlags.Instance);
            detectHeight.SetValue(trackMovement, 0.0f);
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
