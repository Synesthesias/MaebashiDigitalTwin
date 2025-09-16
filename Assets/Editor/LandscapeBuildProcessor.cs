using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Landscape2.Editor.BuildProcessors
{
    /// <summary>
    /// Landscape プロジェクト用の統合ビルドプロセッサー
    /// 各種ビルド時設定を自動化し、一元管理する
    /// </summary>
    public class LandscapeBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        private readonly List<ISubProcessor> subProcessors;

        public LandscapeBuildProcessor()
        {
            // サブプロセッサーを初期化
            subProcessors = new List<ISubProcessor>
            {
                new ShaderProcessor()
            };
        }

        /// <summary>
        /// ビルド前処理の実行
        /// </summary>
        /// <param name="report">ビルドレポート</param>
        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("[LandscapeBuildProcessor] Starting build preprocessing...");

            foreach (var processor in subProcessors)
            {
                try
                {
                    processor.Process();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[LandscapeBuildProcessor] Error in {processor.GetType().Name}: {e.Message}");
                    // 一つのプロセッサーが失敗しても他を続行
                }
            }

            Debug.Log("[LandscapeBuildProcessor] Build preprocessing completed.");
        }
    }

    /// <summary>
    /// サブプロセッサーのインターフェース
    /// </summary>
    public interface ISubProcessor
    {
        /// <summary>
        /// プロセッサー名
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 処理の実行
        /// </summary>
        void Process();
    }
}