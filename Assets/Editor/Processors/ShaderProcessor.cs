using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Landscape2.Editor.BuildProcessors
{
    /// <summary>
    /// シェーダー関連の自動設定を行うサブプロセッサー
    /// </summary>
    public class ShaderProcessor : ISubProcessor
    {
        public string Name => "Shader Processor";

        private const string OUTLINE_SHADER_NAME = "Hidden/Outline";
        private readonly string[] POSSIBLE_OUTLINE_SHADER_PATHS = new string[]
        {
            "Packages/com.synesthesias.plateau-trafficsimulationtool/Runtime/FX/Outline/Outline.shader"
        };

        /// <summary>
        /// シェーダー関連の処理を実行
        /// </summary>
        public void Process()
        {
            Debug.Log($"[{Name}] Processing shader configurations...");

            // Outline シェーダーを Always Included Shaders に追加
            EnsureOutlineShaderIncluded();

            Debug.Log($"[{Name}] Shader processing completed.");
        }

        /// <summary>
        /// Outline シェーダーが Always Included Shaders に含まれているかチェックし、必要に応じて追加
        /// </summary>
        private void EnsureOutlineShaderIncluded()
        {
            try
            {
                // Outline シェーダーを検索して取得
                Shader outlineShader = FindOutlineShader();

                if (outlineShader == null)
                {
                    Debug.LogWarning($"[{Name}] Outline shader '{OUTLINE_SHADER_NAME}' not found in any of the expected paths");
                    return;
                }

                // シェーダー名を確認
                if (outlineShader.name != OUTLINE_SHADER_NAME)
                {
                    Debug.LogWarning($"[{Name}] Shader name mismatch. Expected: {OUTLINE_SHADER_NAME}, Found: {outlineShader.name}");
                }

                // GraphicsSettings の SerializedObject を取得
                SerializedObject serializedGraphicsSettings = new SerializedObject(GraphicsSettings.GetGraphicsSettings());
                SerializedProperty alwaysIncludedShadersProperty = serializedGraphicsSettings.FindProperty("m_AlwaysIncludedShaders");

                if (alwaysIncludedShadersProperty == null)
                {
                    Debug.LogError($"[{Name}] Could not find 'm_AlwaysIncludedShaders' property in GraphicsSettings");
                    return;
                }

                // 既に含まれているかチェック
                bool isAlreadyIncluded = IsShaderAlreadyIncluded(alwaysIncludedShadersProperty, outlineShader);

                if (isAlreadyIncluded)
                {
                    Debug.Log($"[{Name}] {OUTLINE_SHADER_NAME} is already included in Always Included Shaders");
                    return;
                }

                // シェーダーを追加
                if (AddShaderToAlwaysIncluded(alwaysIncludedShadersProperty, outlineShader, serializedGraphicsSettings))
                {
                    Debug.Log($"[{Name}] Successfully added {OUTLINE_SHADER_NAME} to Always Included Shaders");
                }
                else
                {
                    Debug.LogError($"[{Name}] Failed to add {OUTLINE_SHADER_NAME} to Always Included Shaders");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[{Name}] Exception occurred while ensuring Outline shader inclusion: {e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// Outline シェーダーを複数のパスから検索
        /// </summary>
        /// <returns>見つかったシェーダー、または null</returns>
        private Shader FindOutlineShader()
        {
            // 予想されるパスから検索
            foreach (string path in POSSIBLE_OUTLINE_SHADER_PATHS)
            {
                Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                if (shader != null)
                {
                    Debug.Log($"[{Name}] Found Outline shader at: {path}");
                    return shader;
                }
            }

            // Packages フォルダ内でのみ検索（性能改善）
            string[] packagesSearchFolders = { "Packages" };
            string[] guids = AssetDatabase.FindAssets("t:Shader Outline", packagesSearchFolders);
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(assetPath);

                if (shader != null && shader.name == OUTLINE_SHADER_NAME)
                {
                    Debug.Log($"[{Name}] Found Outline shader by search at: {assetPath}");
                    return shader;
                }
            }

            Debug.LogError($"[{Name}] Could not find shader '{OUTLINE_SHADER_NAME}' in Packages folder");
            return null;
        }

        /// <summary>
        /// 指定されたシェーダーが Already Included Shaders に含まれているかチェック
        /// </summary>
        /// <param name="alwaysIncludedShadersProperty">Always Included Shaders のプロパティ</param>
        /// <param name="targetShader">チェック対象のシェーダー</param>
        /// <returns>含まれている場合は true</returns>
        private bool IsShaderAlreadyIncluded(SerializedProperty alwaysIncludedShadersProperty, Shader targetShader)
        {
            for (int i = 0; i < alwaysIncludedShadersProperty.arraySize; i++)
            {
                SerializedProperty shaderProperty = alwaysIncludedShadersProperty.GetArrayElementAtIndex(i);
                Shader existingShader = shaderProperty.objectReferenceValue as Shader;

                if (existingShader != null && existingShader == targetShader)
                {
                    return true;
                }

                // GUIDベースの比較（参照が異なる場合のフォールバック）
                if (existingShader != null && targetShader != null)
                {
                    string existingShaderPath = AssetDatabase.GetAssetPath(existingShader);
                    string targetShaderPath = AssetDatabase.GetAssetPath(targetShader);
                    if (!string.IsNullOrEmpty(existingShaderPath) && !string.IsNullOrEmpty(targetShaderPath))
                    {
                        string existingShaderGUID = AssetDatabase.AssetPathToGUID(existingShaderPath);
                        string targetShaderGUID = AssetDatabase.AssetPathToGUID(targetShaderPath);
                        if (!string.IsNullOrEmpty(existingShaderGUID) && existingShaderGUID == targetShaderGUID)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Always Included Shaders にシェーダーを追加
        /// </summary>
        /// <param name="alwaysIncludedShadersProperty">Always Included Shaders のプロパティ</param>
        /// <param name="shaderToAdd">追加するシェーダー</param>
        /// <param name="serializedGraphicsSettings">SerializedObject</param>
        /// <returns>追加が成功した場合は true</returns>
        private bool AddShaderToAlwaysIncluded(SerializedProperty alwaysIncludedShadersProperty, Shader shaderToAdd, SerializedObject serializedGraphicsSettings)
        {
            try
            {
                // 配列のサイズを増やす
                int newIndex = alwaysIncludedShadersProperty.arraySize;
                alwaysIncludedShadersProperty.InsertArrayElementAtIndex(newIndex);

                // 新しい要素にシェーダーを設定
                SerializedProperty newShaderProperty = alwaysIncludedShadersProperty.GetArrayElementAtIndex(newIndex);
                newShaderProperty.objectReferenceValue = shaderToAdd;

                // 変更を適用
                bool applied = serializedGraphicsSettings.ApplyModifiedProperties();

                if (applied)
                {
                    // アセットデータベースを更新
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }

                return applied;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[{Name}] Exception while adding shader: {e.Message}");
                return false;
            }
        }
    }
}