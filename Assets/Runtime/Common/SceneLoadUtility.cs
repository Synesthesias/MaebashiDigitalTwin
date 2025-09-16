using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Landscape2.Maebashi.Runtime.Common
{
    /// <summary>
    /// シーンロード関連のユーティリティ
    /// </summary>
    public static class SceneLoadUtility
    {
        /// <summary>
        /// 前橋のシーン名定数
        /// </summary>
        public static class MaebashiScenes
        {
            public const string CITY_BUILDINGS = "Maebashi_City_Buildings";
            public const string CITY_ROAD = "Maebashi_City_Road";

            public static readonly string[] ALL = GetValidScenes();

            /// <summary>
            /// 存在するシーンのみを取得（バリデーション機能付き）
            /// </summary>
            private static string[] GetValidScenes()
            {
                var allScenes = new string[] { CITY_BUILDINGS, CITY_ROAD };
                var validScenes = new List<string>();

                foreach (string sceneName in allScenes)
                {
                    if (IsSceneValid(sceneName))
                    {
                        validScenes.Add(sceneName);
                    }
                    else
                    {
                        Debug.LogWarning($"[SceneLoadUtility] Scene '{sceneName}' not found or disabled in build settings");
                    }
                }

                return validScenes.ToArray();
            }

            /// <summary>
            /// シーンが存在し、ビルドに含まれているかチェック
            /// </summary>
            private static bool IsSceneValid(string sceneName)
            {
                // ビルドインデックスでチェック（エディタ・ランタイム共通）
                return SceneUtility.GetBuildIndexByScenePath($"Assets/Maebashi/Scenes/{sceneName}.unity") >= 0;
            }
        }
        /// <summary>
        /// 指定したシーンがすべてロードされているか確認
        /// </summary>
        public static bool AreAllScenesLoaded(params string[] sceneNames)
        {
            if (sceneNames == null || sceneNames.Length == 0) return true;
            
            return sceneNames.All(sceneName =>
            {
                var scene = SceneManager.GetSceneByName(sceneName);
                return scene.IsValid() && scene.isLoaded;
            });
        }
        
        /// <summary>
        /// 指定したシーンがすべてロードされたら処理を実行するイベントハンドラーを登録
        /// </summary>
        public static void RegisterSceneLoadCallback(string[] sceneNames, Action callback)
        {
            // すでに全シーンがロード済みならすぐ実行
            if (AreAllScenesLoaded(sceneNames))
            {
                callback?.Invoke();
                return;
            }
            
            // シーンロードイベントを監視
            void OnSceneLoaded(Scene scene, LoadSceneMode mode)
            {
                if (AreAllScenesLoaded(sceneNames))
                {
                    SceneManager.sceneLoaded -= OnSceneLoaded;
                    callback?.Invoke();
                }
            }
            
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }
}