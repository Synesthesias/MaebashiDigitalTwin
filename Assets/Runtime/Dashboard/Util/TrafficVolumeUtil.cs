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
        public const float MIN_SPAWN_INTERVAL = 2.0f;  // 最小スポーン間隔（秒）
        public const float MAX_SPAWN_INTERVAL = 30.0f; // 最大スポーン間隔（秒）

        private const float BASE_SPEED = 15f; // 最小速度
        private const float SPEED_INCREMENT = 5f; // 各範囲の速度増加量
        private const float MAX_TRAFFIC_SPEED = 45f; // 最大速度

        /// <summary>
        /// 速度からスポーン間隔を計算します
        /// </summary>
        /// <param name="speed">速度（km/h）</param>
        /// <returns>スポーン間隔（秒）</returns>
        public static float CalcSpawnInterval(float speed)
        {
            // 速度を0-1に正規化
            float normalizedSpeed = Mathf.Clamp(speed, 0f, MAX_TRAFFIC_SPEED) / MAX_TRAFFIC_SPEED;
            
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
            // 交通量を200で割り、範囲を決定
            int rangeIndex = Math.Max(0, Math.Min(5, (int)(trafficVolume / 200)));

            // 基本速度に範囲に応じた増加量を加算
            float speed = BASE_SPEED + SPEED_INCREMENT * (5 - rangeIndex);

            // 最大速度を超えないように制限し、m/sに変換して返す
            return Math.Min(speed, MAX_TRAFFIC_SPEED) / 3.6f;
        }
    }
} 