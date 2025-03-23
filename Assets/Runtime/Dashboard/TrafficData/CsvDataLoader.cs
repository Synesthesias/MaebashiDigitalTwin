using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Landscape2.Maebashi.Runtime.Dashboard
{
    public class CsvDataLoader
    {
        /// <summary>
        /// CSVファイルを読み込み、各行のデータを処理する
        /// </summary>
        /// <param name="filePath">Resourcesフォルダ内の相対パス（.csvを除く）</param>
        /// <param name="skipHeader">ヘッダー行をスキップするかどうか</param>
        /// <param name="processLine">各行のデータを処理するデリゲート（nullの場合は文字列配列のまま返す）</param>
        /// <typeparam name="T">変換後のデータ型</typeparam>
        /// <returns>変換されたデータのリスト、またはprocessLineがnullの場合は文字列配列</returns>
        public static async Task<List<T>> LoadCsvData<T>(string filePath, bool skipHeader = false, Func<string[], T> processLine = null)
        {
            try
            {
                // Resources.Loadを使用してテキストアセットとしてCSVを読み込む
                TextAsset csvFile = Resources.Load<TextAsset>(filePath);
                if (csvFile == null)
                {
                    Debug.LogError($"Failed to load CSV file: {filePath}");
                    return new List<T>();
                }

                // テキストを行ごとに分割
                string[] lines = csvFile.text.Split('\n');
                var dataList = new List<T>();
                int startIndex = skipHeader ? 1 : 0;

                // processLineが指定されていない場合は、文字列配列のままリストに追加
                if (processLine == null && typeof(T) == typeof(string[]))
                {
                    for (int i = startIndex; i < lines.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(lines[i]))
                        {
                            dataList.Add((T)(object)lines[i].Split(','));
                        }
                    }
                }
                else
                {
                    for (int i = startIndex; i < lines.Length; i++)
                    {
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(lines[i]))
                            {
                                string[] values = lines[i].Split(',');
                                var data = processLine(values);
                                if (data != null)
                                {
                                    dataList.Add(data);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"Error processing line {i}: {e.Message}");
                        }
                    }
                }

                return dataList;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading CSV file: {ex.Message}");
                return new List<T>();
            }
        }
    }
} 