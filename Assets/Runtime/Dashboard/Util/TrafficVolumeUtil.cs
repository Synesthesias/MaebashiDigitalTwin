using UnityEngine;
using System;

namespace Landscape2.Maebashi.Runtime.Util
{
    /// <summary>
    /// 交通シミュレーションの制御ユーティリティ
    /// </summary>
    public static class TrafficVolumeUtil
    {
        // シミュレーションの基本設定
        public const float MIN_SPAWN_INTERVAL = 2f;  // 最小スポーン間隔（秒）
        public const float MAX_SPAWN_INTERVAL = 30.0f; // 最大スポーン間隔（秒）

        private const float MIN_SPEED = 5f;      // 最小速度 (km/h)
        private const float MAX_SPEED = 40f;     // 最大速度 (km/h)
        private const float MIN_TRAFFIC = 200f;  // 最小交通量の閾値
        private const float MAX_TRAFFIC = 1200f; // 最大交通量の閾値

        /// <summary>
        /// 速度からスポーン間隔を計算します
        /// </summary>
        /// <param name="speed">速度（km/h）</param>
        /// <returns>スポーン間隔（秒）</returns>
        public static float CalcSpawnInterval(float speed)
        {
            // 速度を0-1に正規化
            float normalizedSpeed = Mathf.Clamp(speed, 0f, MAX_SPEED) / MAX_SPEED;
            
            if (normalizedSpeed <= 0f) return 0f; // 速度0の場合はスポーンしない

            // 指数関数的な増加（1.0 → 15.0の範囲で変化）
            float factor = Mathf.Pow(15f, normalizedSpeed);
            
            // 最小2秒から最大30秒の範囲に収める
            return Mathf.Clamp(MIN_SPAWN_INTERVAL * factor, MIN_SPAWN_INTERVAL, MAX_SPAWN_INTERVAL);
        }

        /// <summary>
        /// 交通量に基づいて速度を計算します
        /// </summary>
        /// <param name="trafficVolume">交通量</param>
        /// <returns>速度（m/s）</returns>
        public static float CalculateTrafficSpeed(float trafficVolume)
        {
            // 交通量を正規化（200-1200の範囲を0-1に変換）
            float normalizedTraffic = Mathf.Clamp01((trafficVolume - MIN_TRAFFIC) / (MAX_TRAFFIC - MIN_TRAFFIC));
            
            // 正規化された値を反転（交通量が多いほど速度が遅くなる）
            float speedFactor = 1f - normalizedTraffic;
            
            // 速度を計算（5-40 km/hの範囲）
            float speed = Mathf.Lerp(MIN_SPEED, MAX_SPEED, speedFactor);
            
            // km/hからm/sに変換して返す
            return speed / 3.6f;
        }

        /// <summary>
        /// 1時間あたりの交通量からスポーン間隔を計算
        /// </summary>
        /// <param name="trafficVolume">1時間あたりの交通量</param>
        /// <returns>スポーン間隔（秒）</returns>
        public static float CalculateSpawnInterval(float trafficVolume)
        {
            if (trafficVolume <= 0f) return MAX_SPAWN_INTERVAL;

            // 1時間あたりの交通量から1台あたりのスポーン間隔（秒）を計算
            // 例: 交通量360台/時 → 10秒間隔でスポーン (3600秒 ÷ 360台 = 10秒/台)
            float spawnInterval = 3600f / trafficVolume;
            
            // 最小2秒から最大30秒の範囲に収める
            return Mathf.Clamp(spawnInterval, MIN_SPAWN_INTERVAL, MAX_SPAWN_INTERVAL);
        }
    }
} 