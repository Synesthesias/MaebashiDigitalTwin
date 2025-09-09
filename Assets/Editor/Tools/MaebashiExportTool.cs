using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using CompressionLevel = System.IO.Compression.CompressionLevel;
using Debug = UnityEngine.Debug;

/// <summary>
/// MaebashiフォルダとPackagesをエクスポートするツール
/// </summary>
public class MaebashiExportTool : EditorWindow
{
    private string outputPath = "";
    private bool isProcessing = false;
    private bool isCompleted = false;
    
    // ZIP用プログレス
    private float zipProgress = 0f;
    private string zipProgressMessage = "";
    
    // TGZ用プログレス
    private float tgzProgress = 0f;
    private string tgzProgressMessage = "";
    
    // 時間計測用
    private Stopwatch overallStopwatch;
    
    private CancellationTokenSource cancellationTokenSource;
    private Task currentTask;
    
    
    // パッケージリスト
    private readonly string[] packageNames = new[]
    {
        "landscape-design-tool",
        "PLATEAU-SDK-for-Unity",
        "PLATEAU-SDK-Toolkits-for-Unity",
        "Maps-Toolkit-for-Unity-Landscape",
        "Data-Preparation-Tool-for-TrafficSim"
    };

    [MenuItem("Tools/Maebashiエクスポート")]
    public static void ShowWindow()
    {
        var window = GetWindow<MaebashiExportTool>("Maebashiエクスポート");
        window.minSize = new Vector2(500, 200);
    }

    private void OnGUI()
    {
        GUILayout.Label("Maebashi & Packages エクスポートツール", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // 出力先フォルダ選択
        GUILayout.Label("出力先フォルダ:");
        using (new GUILayout.HorizontalScope())
        {
            outputPath = EditorGUILayout.TextField(outputPath);
            if (GUILayout.Button("選択", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("出力先フォルダを選択", outputPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    outputPath = path;
                }
            }
        }

        GUILayout.Space(10);

        // エクスポートボタン
        if (isCompleted)
        {
            GUI.enabled = false;
            GUILayout.Button("完了", GUILayout.Height(30));
            GUI.enabled = true;
        }
        else
        {
            GUI.enabled = !isProcessing && !string.IsNullOrEmpty(outputPath);
            if (GUILayout.Button("エクスポート", GUILayout.Height(30)))
            {
                StartExport();
            }
            GUI.enabled = true;
        }

        // 処理中の表示
        if (isProcessing)
        {
            GUILayout.Space(10);
            
            // 全体の経過時間表示
            if (overallStopwatch != null)
            {
                string timeLabel = isCompleted ? "完了時間" : "経過時間";
                GUILayout.Label($"{timeLabel}: {FormatElapsedTime(overallStopwatch.Elapsed)}", EditorStyles.boldLabel);
                GUILayout.Space(5);
            }
            
            // Maebashi ZIP プログレスバー
            GUILayout.Label("Maebashi ZIP:");
            EditorGUI.ProgressBar(
                EditorGUILayout.GetControlRect(GUILayout.Height(20)),
                zipProgress,
                zipProgressMessage
            );
            
            GUILayout.Space(5);
            
            // Packages TGZ プログレスバー
            GUILayout.Label("Packages TGZ:");
            EditorGUI.ProgressBar(
                EditorGUILayout.GetControlRect(GUILayout.Height(20)),
                tgzProgress,
                tgzProgressMessage
            );
            
            GUILayout.Space(5);
            
            // キャンセルボタン
            if (GUILayout.Button("キャンセル", GUILayout.Height(25)))
            {
                CancelExport();
            }
        }
    }

    private async void StartExport()
    {
        if (!Directory.Exists(outputPath))
        {
            EditorUtility.DisplayDialog("エラー", "出力先フォルダが存在しません。", "OK");
            return;
        }

        // ディスク容量チェック（簡易版）
        var driveInfo = new DriveInfo(Path.GetPathRoot(outputPath));
        if (driveInfo.AvailableFreeSpace < 10L * 1024 * 1024 * 1024) // 10GB以上の空き容量が必要
        {
            bool proceed = EditorUtility.DisplayDialog(
                "警告",
                "ディスクの空き容量が少ない可能性があります。続行しますか？",
                "続行", "キャンセル"
            );
            if (!proceed) return;
        }

        isProcessing = true;
        zipProgress = 0f;
        zipProgressMessage = "準備中...";
        tgzProgress = 0f;
        tgzProgressMessage = "準備中...";
        
        
        // 全体のStopwatchを開始
        overallStopwatch = new Stopwatch();
        overallStopwatch.Start();
        
        cancellationTokenSource = new CancellationTokenSource();
        
        try
        {
            Debug.Log("[MaebashiExportTool] エクスポート開始");
            currentTask = ExportAllAsync(cancellationTokenSource.Token);
            await currentTask;
            Debug.Log("[MaebashiExportTool] ExportAllAsync完了");
            
            Debug.Log($"[MaebashiExportTool] キャンセレーション状態チェック: {cancellationTokenSource.Token.IsCancellationRequested}");
            if (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                Debug.Log("[MaebashiExportTool] 正常完了 - ダイアログ表示します");
                isCompleted = true;
                overallStopwatch?.Stop(); // 経過時間を停止
                EditorUtility.DisplayDialog("完了", "エクスポートが完了しました。", "OK");
                EditorUtility.RevealInFinder(outputPath);
            }
        }
        catch (OperationCanceledException)
        {
            EditorUtility.DisplayDialog("キャンセル", "エクスポートがキャンセルされました。", "OK");
            CleanupPartialFiles();
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("エラー", $"エクスポート中にエラーが発生しました:\n{ex.Message}", "OK");
            Debug.LogError($"Export error: {ex}");
            CleanupPartialFiles();
        }
        finally
        {
            if (!isCompleted)
            {
                isProcessing = false;
                zipProgress = 0f;
                zipProgressMessage = "";
                tgzProgress = 0f;
                tgzProgressMessage = "";
                overallStopwatch?.Stop();
            }
            else
            {
                isProcessing = false; // 完了時もprocessingは終了
            }
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
            currentTask = null;
            Repaint();
        }
    }

    private async Task ExportAllAsync(CancellationToken cancellationToken)
    {
        Debug.Log("[MaebashiExportTool] ExportAllAsync開始");
        // 1. MaebashiフォルダのZIP化（別スレッドで実行可能）
        string maebashiPath = Path.Combine(Application.dataPath, "Maebashi");
        Task zipTask = null;
        if (Directory.Exists(maebashiPath))
        {
            string zipPath = Path.Combine(outputPath, "Maebashi.zip");
            zipTask = CreateZipAsync(maebashiPath, zipPath, cancellationToken);
        }
        
        // 2. PackagesのTGZ化（メインスレッドで順次実行）
        
        // 全体の経過時間の定期更新タスク
        var progressUpdateTask = Task.Run(async () =>
        {
            while (overallStopwatch != null && overallStopwatch.IsRunning && !cancellationToken.IsCancellationRequested)
            {
                // UI更新（経過時間表示のため）
                await Task.Delay(1000, cancellationToken); // 1秒ごと更新
                if (overallStopwatch.IsRunning) // まだ実行中の場合のみ更新
                {
                    EditorApplication.delayCall += () => Repaint();
                }
            }
        }, cancellationToken);
        
        int packageIndex = 0;
        foreach (var packageName in packageNames)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            string packagePath = $"Packages/{packageName}";
            string fullPackagePath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Packages", packageName));
            
            if (Directory.Exists(fullPackagePath))
            {
                string tgzPath = Path.Combine(outputPath, $"{packageName}.tgz");
                await CreateTgzOnMainThreadAsync(packagePath, tgzPath, packageName, packageIndex, packageNames.Length, cancellationToken);
                packageIndex++;
            }
        }
        
        // ZIP化の完了を待つ
        if (zipTask != null)
        {
            Debug.Log("[MaebashiExportTool] ZIP完了待機中");
            await zipTask;
            Debug.Log("[MaebashiExportTool] ZIP完了");
        }
        Debug.Log("[MaebashiExportTool] ExportAllAsync全て完了");
    }

    private async Task CreateZipAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            try
            {
                UpdateZipProgress(0.1f, "ZIP作成開始(約10分)...");
                
                // .NET標準の高速メソッドを使用
                ZipFile.CreateFromDirectory(
                    sourcePath,                        // Assets/Maebashi
                    destinationPath,                   // 出力先/Maebashi.zip
                    CompressionLevel.NoCompression,    // 圧縮なしで高速化
                    false                              // ベースディレクトリを含めない
                );
                
                UpdateZipProgress(1.0f, "ZIP完了");
                Debug.Log("[MaebashiExportTool] ZIP作成成功");
            }
            catch (Exception ex)
            {
                Debug.LogError($"ZIP creation error: {ex}");
                throw;
            }
        }, cancellationToken);
    }

    private async Task CreateTgzOnMainThreadAsync(string sourcePath, string destinationPath, string packageName, int packageIndex, int totalPackages, CancellationToken cancellationToken)
    {
        float baseProgress = (float)packageIndex / totalPackages;
        UpdateTgzProgress(baseProgress, $"{packageName} TGZ作成中... ({packageIndex + 1}/{totalPackages})");
        
        try
        {
            // Unity Package Manager のClient.Packを使用してTGZ作成（メインスレッドで実行）
            // sourcePathは "Packages/package-name" 形式
            var request = Client.Pack(sourcePath, outputPath);
            
            // リクエストが完了するまで待機
            while (!request.IsCompleted)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield(); // メインスレッドでの非同期待機
            }
            
            if (request.Status == StatusCode.Success)
            {
                float completedProgress = (float)(packageIndex + 1) / totalPackages;
                UpdateTgzProgress(completedProgress, $"{packageName} TGZ完了 ({packageIndex + 1}/{totalPackages})");
                Debug.Log($"[MaebashiExportTool] {packageName} TGZ作成成功");
            }
            else
            {
                throw new Exception($"Package pack failed: {request.Error?.message}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"TGZ creation error: {ex.Message}");
            throw;
        }
    }

    private void CancelExport()
    {
        if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
        {
            cancellationTokenSource.Cancel();
            zipProgressMessage = "キャンセル中...";
            tgzProgressMessage = "キャンセル中...";
        }
    }

    private void CleanupPartialFiles()
    {
        // 部分的に作成されたファイルを削除
        try
        {
            // 日付なしのファイル名パターン
            var patterns = new[] { "Maebashi.zip", "*.tgz" };
            foreach (var pattern in patterns)
            {
                var files = Directory.GetFiles(outputPath, pattern);
                foreach (var file in files)
                {
                    // エクスポート中に作成された可能性のあるファイルを削除
                    if (File.Exists(file))
                    {
                        var fileInfo = new FileInfo(file);
                        // 最近作成された（1時間以内）ファイルのみ削除
                        if ((DateTime.Now - fileInfo.CreationTime).TotalHours < 1)
                        {
                            File.Delete(file);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Cleanup failed: {ex.Message}");
        }
    }

    private void UpdateZipProgress(float value, string message)
    {
        zipProgress = value;
        zipProgressMessage = message;
        
        // メインスレッドで実行
        EditorApplication.delayCall += () => Repaint();
    }
    
    private void UpdateTgzProgress(float value, string message)
    {
        tgzProgress = value;
        tgzProgressMessage = message;
        
        // メインスレッドで実行
        EditorApplication.delayCall += () => Repaint();
    }
    
    private string FormatElapsedTime(TimeSpan elapsed)
    {
        if (elapsed.TotalMinutes < 1)
        {
            return $"{elapsed.Seconds}秒";
        }
        else if (elapsed.TotalHours < 1)
        {
            return $"{elapsed.Minutes}分{elapsed.Seconds}秒";
        }
        else
        {
            return $"{elapsed.Hours}時間{elapsed.Minutes}分";
        }
    }

    private void OnDestroy()
    {
        // ウィンドウが閉じられた場合はキャンセル
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
    }
}