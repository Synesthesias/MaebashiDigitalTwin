using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Landscape2.Maebashi.Runtime.Dashboard
{
    /// <summary>
    /// 人流量データ
    /// 交差点ごとに存在する
    /// 進行方向＞時間帯＞人流量　を扱う
    /// </summary>
    [Serializable]
    public class HumanFlowData
    {
        public static readonly int DATA_TIME_START = 7; // 7:00
        public static readonly int DATA_TIME_END = 20; // 20:00
        public static readonly int DATA_TIME_RANGE = DATA_TIME_END - DATA_TIME_START;

        /// <summary>
        /// 時間帯ごとに格納された人流量
        /// 時間帯の配列に人流量が含まれる
        /// </summary>
        [Serializable]
        public class HumanFlowDirectionData
        {
            public int[] amount;
        }

        /// <summary>
        /// 人流量データ
        /// 方向ごとに人流量データを持つ
        /// </summary>
        [SerializeField]
        private List<HumanFlowDirectionData> humanFlowDirectionData;

        /// <summary>
        /// 人流量データの数
        /// </summary>
        public int CountDir => humanFlowDirectionData?.Count ?? 0;

        /// <summary>
        /// .csvは不要
        /// </summary>
        /// <param name="path"></param>
        public void Load(TextAsset asset)
        {
            var data = CsvDataLoader.LoadCsvData<HumanFlowDirectionData>(asset, true, values =>
            {
                if (values.Length < DATA_TIME_RANGE) 
                    return new HumanFlowDirectionData();

                return new HumanFlowDirectionData
                {
                    amount = Array.ConvertAll(values, int.Parse)
                };
            });

            humanFlowDirectionData = data;
        }

        /// <summary>
        /// データ取得
        /// dataIdxは一意な値ではないため人による紐づけが必要
        /// </summary>
        /// <param name="dataIdx">歩道の進行方向ごとに存在するIdx</param>
        /// <param name="time"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public int Read(int dataIdx, int time)
        {
            var timeId = time - DATA_TIME_START;
            if (humanFlowDirectionData == null || dataIdx < 0 || dataIdx >= humanFlowDirectionData.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(dataIdx), "Invalid dataId or humanFlowAmounts not loaded.");
            }

            if (timeId < 0 || timeId >= DATA_TIME_RANGE)
            {
                throw new ArgumentOutOfRangeException(nameof(timeId), "Invalid timeId.");
            }

            return humanFlowDirectionData[dataIdx].amount[timeId];
        }
    }

}
